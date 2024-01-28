using FishingMap.Domain.Data.DTO.UserObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Interfaces
{
    public interface IUserService
    {
        Task<UserDTO> AddUser(UserAdd user);
        Task<UserDTO> AddAdministrator(UserAdd user);
        Task DeleteUser(int id);
        Task<UserDTO> GetUser(int id);
        Task<UserDTO> GetUserByEmail(string email);
        Task<UserDTO> GetUserByUsername(string username);
        Task<UserCredentials> GetUserCredentials(int id);
        Task<IEnumerable<UserDTO>> GetUsers();
        Task<UserDTO> UpdateUserDetails(int id, UserDetailsUpdate user);
        Task<bool> UpdateUserPassword(int id, string password);

    }
}
