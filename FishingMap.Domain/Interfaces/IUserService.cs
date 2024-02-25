using FishingMap.Domain.DTO.Users;

namespace FishingMap.Domain.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO?> AddUser(UserAdd user);
        Task<UserDTO?> AddAdministrator(UserAdd user);
        Task DeleteUser(int id);
        Task<UserDTO?> GetUser(int id);
        Task<UserDTO?> GetUserByEmail(string email);
        Task<UserDTO?> GetUserByUsername(string username);
        Task<UserCredentials?> GetUserCredentials(int id);
        Task<UserCredentials?> GetUserCredentialsByUserName(string username);
        Task<IEnumerable<UserDTO>> GetUsers();
        Task<UserDTO> UpdateUserDetails(int id, UserDetailsUpdate user);
        Task<bool> UpdateUserPassword(int id, string password);

    }
}
