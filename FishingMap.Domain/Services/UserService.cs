using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FishingMap.Common.Utils;
using FishingMap.Domain.Interfaces;
using FishingMap.Data.Interfaces;
using FishingMap.Data.Entities;
using FishingMap.Domain.DTO.Users;

namespace FishingMap.Domain.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserDTO?> AddUser(UserAdd user)
        {
            if (!string.IsNullOrWhiteSpace(user.Password) &&
                !await _unitOfWork.Users.Any(u =>
                    u.UserName == user.UserName || u.Email == user.Email)
                )
            {
                var roles = await _unitOfWork.Roles
                    .GetAll(r => r.Name == "User");
                    
                var newUser = await AddUserToDb(user, roles.ToArray());

                return _mapper.Map<UserDTO>(newUser);
            }

            return null;
        }

        public async Task<UserDTO?> AddAdministrator(UserAdd user)
        {
            if (!string.IsNullOrWhiteSpace(user.Password) &&
                !await _unitOfWork.Users.Any(u =>
                    u.UserName == user.UserName || u.Email == user.Email)
                )
            {
                var roles = await _unitOfWork.Roles
                    .GetAll(r => r.Name == "Administrator" || r.Name == "User", noTracking: true);

                var newUser = await AddUserToDb(user, roles.ToArray());

                return _mapper.Map<UserDTO>(newUser);
            }

            return null;
        }

        public async Task DeleteUser(int id)
        {
            await _unitOfWork.Users.Delete(id);
            await _unitOfWork.SaveChanges();
        }

        public async Task<UserDTO?> GetUser(int id)
        {
            var user = await _unitOfWork.Users.GetUserWithRoles(id, true);

            if (user != null)
            {
                return _mapper.Map<UserDTO>(user);
            }
            return null;
        }

        public async Task<UserDTO?> GetUserByEmail(string email)
        {
            var user = await _unitOfWork.Users.Find(
                                u => u.Email == email,
                                [u => u.Roles.OrderBy(r => r.Name)],
                                true);

            if (user != null)
            {
                return _mapper.Map<UserDTO>(user);
            }
            return null;
        }

        public async Task<UserDTO?> GetUserByUsername(string username)
        {
            var user = await _unitOfWork.Users.Find(
                                u => u.UserName == username,
                                [s => s.Roles.OrderBy(r => r.Name)], 
                                true);

            if (user != null)
            {
                return _mapper.Map<UserDTO>(user);
            }

            return null;
        }

        public async Task<UserCredentials?> GetUserCredentials(int id)
        {
            var user = await _unitOfWork.Users.GetById(id, noTracking: true);
            if (user != null)
            {
                return _mapper.Map<UserCredentials>(user);
            }

            return null;
        }

        public async Task<IEnumerable<UserDTO>> GetUsers()
        {
            var users = await _unitOfWork.Users.GetAll(
                                null,
                                [u => u.Roles.OrderBy(r => r.Name)],
                                u => u.OrderBy(u => u.UserName));

            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<UserDTO?> UpdateUserDetails(int id, UserDetailsUpdate user)
        {
            var userEntity = await _unitOfWork.Users.GetById(id);
            if (userEntity != null)
            {
                userEntity.FirstName = user.FirstName;
                userEntity.LastName = user.LastName;
                userEntity.Email = user.Email;
                userEntity.Modified = DateTime.Now;

                await _unitOfWork.SaveChanges();

                return _mapper.Map<UserDTO>(userEntity);
            }

            return null;
        }

        public async Task<bool> UpdateUserPassword(int id, string password)
        {
            var userEntity = await _unitOfWork.Users.GetById(id);
            if (userEntity != null)
            {
                userEntity.Password = Cryptography.CreateHash(password, userEntity.Salt);
                userEntity.Modified = DateTime.Now;
                await _unitOfWork.SaveChanges();

                return true;
            }

            return false;
        }

        private async Task<User> AddUserToDb(UserAdd user, Role[] roles)
        {
            var passwordSalt = Cryptography.CreateSalt();
            var passwordHash = Cryptography.CreateHash(user.Password, passwordSalt); ;

            var entity = new User()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Password = passwordHash,
                Salt = passwordSalt,
                Roles = roles
            };

            var now = DateTime.Now;
            entity.Created = now;
            entity.Modified = now;

            entity = _unitOfWork.Users.Add(entity);
            await _unitOfWork.SaveChanges();

            return entity;
        }
    }
}
