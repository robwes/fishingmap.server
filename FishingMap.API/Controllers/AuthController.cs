﻿using FishingMap.API.Interfaces;
using FishingMap.Domain.DTO.Users;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FishingMap.API.Controllers
{
    public class UserModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? EmailAddress { get; set; }
        public string? Role { get; set; }
        public string? Surname { get; set; }
        public string? GivenName { get; set; }
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
        public async Task<ActionResult<UserDTO>> Login([FromBody]UserLogin userLogin)
        {
            try
            {
                var user = await _userService.GetUserByUsername(userLogin.UserName);
                if (user == null)
                {
                    return NotFound();
                }

                var userCredentials = await _userService.GetUserCredentialsByUserName(userLogin.UserName);
                if (userCredentials == null)
                {
                    return NotFound();
                }

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
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                Response.Cookies.Delete("token", new CookieOptions
                {
                    HttpOnly = true,
                    SameSite = SameSiteMode.None,
                    Secure = true
                });
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("whoami")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> WhoAmI()
        {
            try
            {
                var currentUserIdentity = GetCurrentUserIdentity();
                if (currentUserIdentity != null && currentUserIdentity.Username != null)
                {
                    var currentUser = await _userService.GetUserByUsername(currentUserIdentity.Username);
                    if (currentUser != null)
                    {
                        return Ok(currentUser);
                    }
                }

                return Unauthorized();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
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
