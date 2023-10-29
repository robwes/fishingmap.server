using FishingMap.Domain.Data.DTO.UserObjects;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FishingMap.API.Interfaces
{
    public interface IAuthService
    {
        string GenerateToken(User user);
        Task<User> GetCurrentUser(HttpContext httpContext);
        bool ValidateUserPassword(UserCredentials userCredentials, string password);
    }
}
