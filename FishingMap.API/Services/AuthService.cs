using FishingMap.API.Interfaces;
using FishingMap.Common.Utils;
using FishingMap.Domain.DTO.Users;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _config;
        private readonly IUserService _userService;

        public AuthService(IConfiguration config, IUserService userService)
        {
            _config = config;
            _userService = userService;
        }

        public string GenerateToken(UserDTO user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); ;

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            if (user.FirstName != null)
            {
                claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            }

            if (user.LastName != null)
            {
                claims.Add(new Claim(ClaimTypes.Surname, user.LastName));
            }

            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }
            
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Audience"],
              claims.ToArray(),
              expires: DateTime.Now.AddMinutes(60),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<UserDTO> GetCurrentUser(HttpContext httpContext)
        {
            var identity = httpContext.User.Identity as ClaimsIdentity;
            if (identity?.Claims?.Count() > 0)
            {
                var username = identity.Claims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value;
                if (username != null)
                {
                    return await _userService.GetUserByUsername(username);
                }
            }
            return null;
        }

        public bool ValidateUserPassword(UserCredentials userCredentials, string password)
        {
            return Cryptography.Validate(password, userCredentials.Salt, userCredentials.Password);
        }
    }
}
