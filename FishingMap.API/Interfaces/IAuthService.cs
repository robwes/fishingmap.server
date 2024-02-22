using FishingMap.Domain.DTO.Users;

namespace FishingMap.API.Interfaces
{
    public interface IAuthService
    {
        string GenerateToken(UserDTO user);
        Task<UserDTO?> GetCurrentUser(HttpContext httpContext);
        bool ValidateUserPassword(UserCredentials userCredentials, string password);
    }
}
