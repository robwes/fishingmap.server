using FishingMap.Domain.Data.DTO;

namespace FishingMap.API.Interfaces
{
    public interface IAuthService
    {
        string GenerateToken(User user);
        bool ValidateUserPassword(User user, string password);
    }
}
