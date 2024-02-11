using FishingMap.Domain.DTO.Users;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FishingMap.API.Interfaces
{
    public interface IAuthService
    {
        string GenerateToken(UserDTO user);
        Task<UserDTO> GetCurrentUser(HttpContext httpContext);
        bool ValidateUserPassword(UserCredentials userCredentials, string password);
    }
}
