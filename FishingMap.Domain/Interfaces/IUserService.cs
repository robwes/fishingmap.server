using FishingMap.Domain.Data.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Interfaces
{
    public interface IUserService
    {
        Task<User> AddUser(UserRegister user);
        Task<User> AddAdministrator(UserRegister user);
        Task<User> UpdateUser(int id, User user);
        Task DeleteUser(int id);
        Task<IEnumerable<User>> GetUsers();
        Task<User> GetUser(int id);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserByUsername(string username);
    }
}
