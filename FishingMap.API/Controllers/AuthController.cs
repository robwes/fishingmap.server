using FishingMap.API.Interfaces;
using FishingMap.Common.Utils;
using FishingMap.Domain.DTO.Users;
using FishingMap.Domain.Interfaces;
using FishingMap.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FishingMap.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private const string AccessTokenCookie = "token";
        private const string RefreshTokenCookie = "refreshToken";
        private const string RefreshTokenCookiePath = "/api/auth";

        // Used to burn the same PBKDF2 time when the username doesn't exist, so
        // response timing doesn't reveal which usernames are registered.
        private static readonly string TimingEqualizationSalt = Cryptography.CreateSalt();
        private static readonly string TimingEqualizationHash =
            Cryptography.CreateHash(Guid.NewGuid().ToString(), TimingEqualizationSalt);

        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IWebHostEnvironment _environment;

        public AuthController(
            IAuthService authService,
            IUserService userService,
            IRefreshTokenService refreshTokenService,
            IWebHostEnvironment environment)
        {
            _authService = authService;
            _userService = userService;
            _refreshTokenService = refreshTokenService;
            _environment = environment;
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<ActionResult<UserDTO>> Login([FromBody]UserLogin userLogin)
        {
            // Same response for unknown user and wrong password — don't leak which usernames exist
            var user = await _userService.GetUserByUsername(userLogin.UserName);
            var userCredentials = await _userService.GetUserCredentialsByUserName(userLogin.UserName);
            if (user == null || userCredentials == null)
            {
                Cryptography.Validate(userLogin.Password, TimingEqualizationSalt, TimingEqualizationHash);
                return BadRequest(new { message = "Invalid credentials" });
            }

            if (!_authService.ValidateUserPassword(userCredentials, userLogin.Password, out var needsRehash))
            {
                return BadRequest(new { message = "Invalid credentials" });
            }

            if (needsRehash)
            {
                // Hash was created with the old, weaker iteration count — upgrade it
                // now while we have the plaintext password.
                await _userService.UpdateUserPassword(user.Id, userLogin.Password);
            }

            var jwtToken = _authService.GenerateToken(user);
            var refreshToken = await _refreshTokenService.IssueToken(user.Id);
            SetAuthCookies(jwtToken, refreshToken);

            return Ok(user);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<UserDTO>> Refresh()
        {
            var rawToken = Request.Cookies[RefreshTokenCookie];
            if (string.IsNullOrEmpty(rawToken))
            {
                return Unauthorized();
            }

            var rotated = await _refreshTokenService.RotateToken(rawToken);
            if (rotated == null)
            {
                DeleteAuthCookies();
                return Unauthorized();
            }

            var user = await _userService.GetUser(rotated.Value.UserId);
            if (user == null)
            {
                DeleteAuthCookies();
                return Unauthorized();
            }

            var jwtToken = _authService.GenerateToken(user);
            SetAuthCookies(jwtToken, rotated.Value.NewToken);

            return Ok(user);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var rawToken = Request.Cookies[RefreshTokenCookie];
            if (!string.IsNullOrEmpty(rawToken))
            {
                await _refreshTokenService.RevokeToken(rawToken);
            }

            DeleteAuthCookies();
            return Ok();
        }

        [HttpGet("whoami")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> WhoAmI()
        {
            var currentUser = await _authService.GetCurrentUser(HttpContext);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            return Ok(currentUser);
        }

        private void SetAuthCookies(string jwtToken, string refreshToken)
        {
            Response.Cookies.Append(AccessTokenCookie, jwtToken, AccessCookieOptions());
            Response.Cookies.Append(RefreshTokenCookie, refreshToken, RefreshCookieOptions());
        }

        private void DeleteAuthCookies()
        {
            Response.Cookies.Delete(AccessTokenCookie, AccessCookieOptions());
            Response.Cookies.Delete(RefreshTokenCookie, RefreshCookieOptions());
        }

        private CookieOptions AccessCookieOptions() => new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = CookieSameSite()
        };

        private CookieOptions RefreshCookieOptions() => new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = CookieSameSite(),
            // Only sent to the auth endpoints that actually need it
            Path = RefreshTokenCookiePath,
            Expires = DateTimeOffset.UtcNow.AddDays(RefreshTokenService.LifetimeDays)
        };

        // Lax blocks cross-site requests from carrying the cookies (CSRF): the SPA at
        // fishingmap.fi and the API at api.fishingmap.fi are same-site. Development runs
        // http://localhost:3000 → https://localhost:7299, which schemeful same-site
        // treats as cross-site, so Lax would break local login — keep None there.
        private SameSiteMode CookieSameSite() =>
            _environment.IsDevelopment() ? SameSiteMode.None : SameSiteMode.Lax;
    }
}
