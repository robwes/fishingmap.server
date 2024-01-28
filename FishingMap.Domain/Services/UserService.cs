using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FishingMap.Common.Utils;
using FishingMap.Domain.Interfaces;
using FishingMap.Domain.Data.DTO.UserObjects;
using FishingMap.Data.Interfaces;
using FishingMap.Data.Entities;

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

        public async Task<UserDTO> AddUser(UserAdd user)
        {
            if (!string.IsNullOrWhiteSpace(user.Password) &&
                !await _unitOfWork.Users.Any(u =>
                    u.UserName == user.UserName || u.Email == user.Email)
                )
            {
                var roles = await _unitOfWork.Roles
                    .GetAll(r => r.Name == "User");
                    
                var newUser = AddUserToDb(user, roles.ToArray());

                return _mapper.Map<UserDTO>(newUser);
            }

            return null;
        }

        public async Task<UserDTO> AddAdministrator(UserAdd user)
        {
            if (!string.IsNullOrWhiteSpace(user.Password) &&
                !await _unitOfWork.Users.Any(u =>
                    u.UserName == user.UserName || u.Email == user.Email)
                )
            {
                var roles = await _unitOfWork.Roles
                    .GetAll(r => r.Name == "Administrator" || r.Name == "User");

                var newUser = AddUserToDb(user, roles.ToArray());

                return _mapper.Map<UserDTO>(newUser);
            }

            return null;
        }

        public async Task DeleteUser(int id)
        {
            await _unitOfWork.Users.Delete(id);
            await _unitOfWork.SaveChanges();
        }

        public async Task<UserDTO> GetUser(int id)
        {
            var user = await _unitOfWork.Users.GetById(
                id, 
                new string[] {"Roles"});

            if (user != null)
            {
                return _mapper.Map<UserDTO>(user);
            }
            return null;
        }

        public async Task<UserDTO> GetUserByEmail(string email)
        {
            var user = await _unitOfWork.Users.Find(
                                u => u.Email == email,
                                new string[] {"Roles"});

            if (user != null)
            {
                return _mapper.Map<UserDTO>(user);
            }
            return null;
        }

        public async Task<UserDTO> GetUserByUsername(string username)
        {
            var user = await _unitOfWork.Users.Find(
                                u => u.UserName == username,
                                new string[] {"Roles"});

            if (user != null)
            {
                return _mapper.Map<UserDTO>(user);
            }

            return null;
        }

        public async Task<UserCredentials> GetUserCredentials(int id)
        {
            var user = await _unitOfWork.Users.GetById(id);
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
                                u => u.OrderBy(u => u.UserName),
                                new string[] {"Roles"});

            return _mapper.Map<IEnumerable<UserDTO>>(users);
        }

        public async Task<UserDTO> UpdateUserDetails(int id, UserDetailsUpdate user)
        {
            var userEntity = await _unitOfWork.Users.GetById(id);
            if (userEntity != null)
            {
                userEntity.FirstName = user.FirstName;
                userEntity.LastName = user.LastName;
                userEntity.Email = user.Email;
                userEntity.Modified = DateTime.Now;

                userEntity = _unitOfWork.Users.Update(userEntity);
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

        private async Task<User> AddUserToDb(UserAdd user, FishingMap.Data.Entities.Role[] roles)
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
