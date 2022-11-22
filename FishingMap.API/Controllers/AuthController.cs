using FishingMap.API.Interfaces;
using FishingMap.Domain.Data.DTO;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FishingMap.API.Controllers
{
    public class UserModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string EmailAddress { get; set; }
        public string Role { get; set; }
        public string Surname { get; set; }
        public string GivenName { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]UserLogin userLogin)
        {
            var user = await _userService.GetUserByUsername(userLogin.UserName);
            if (user == null)
            {
                return NotFound();
            }

            var userCredentials = await _userService.GetUserCredentials(user.Id);
            if (!_authService.ValidateUserPassword(userCredentials, userLogin.Password))
            {
                return BadRequest(new { message = "Invalid credentials" });
            }

            var jwtToken = _authService.GenerateToken(user);
            Response.Cookies.Append("token", jwtToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true
            });

            return Ok(user);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("token", new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true
            });
            return Ok();
        }

        [HttpGet("whoami")]
        [Authorize]
        public async Task<IActionResult> WhoAmI()
        {
            var currentUserIdentity = GetCurrentUserIdentity();
            if (currentUserIdentity != null)
            {
                var currentUser = await _userService.GetUserByUsername(currentUserIdentity.Username);
                if (currentUser != null)
                {
                    return Ok(currentUser);
                }
            }

            return Unauthorized();
        }

        private UserModel? GetCurrentUserIdentity()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;

            if (identity?.Claims?.Count() > 0)
            {
                var userClaims = identity.Claims;

                return new UserModel
                {
                    Username = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
                    EmailAddress = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value,
                    GivenName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.GivenName)?.Value,
                    Surname = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Surname)?.Value,
                    Role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value
                };
            }
            return null;
        }
    }
}
