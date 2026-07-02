using FishingMap.API.Interfaces;
using FishingMap.Common.Utils;
using FishingMap.Domain.DTO.Users;
using FishingMap.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); ;

            var claims = new List<Claim>()
            {
                // Id (not username) identifies the user so renames don't orphan sessions
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
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
              expires: DateTime.UtcNow.AddMinutes(60),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<UserDTO?> GetCurrentUser(HttpContext httpContext)
        {
            var idClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idClaim, out var userId))
            {
                return await _userService.GetUser(userId);
            }
            return null;
        }

        public bool ValidateUserPassword(UserCredentials userCredentials, string password)
        {
            return Cryptography.Validate(password, userCredentials.Salt, userCredentials.Password);
        }

        public bool ValidateUserPassword(UserCredentials userCredentials, string password, out bool needsRehash)
        {
            return Cryptography.Validate(password, userCredentials.Salt, userCredentials.Password, out needsRehash);
        }
    }
}
