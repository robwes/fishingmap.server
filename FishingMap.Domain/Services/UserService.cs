using AutoMapper;
using FishingMap.Domain.Data.Context;
using FishingMap.Domain.Data.DTO;
using FishingMap.Domain.Utils;
using FishingMap.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FishingMap.Domain.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UserService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<User> AddUser(UserRegister user)
        {
            if (!string.IsNullOrWhiteSpace(user.Password) &&
                !await _context.Users.AnyAsync(u =>
                    u.UserName == user.UserName || u.Email == user.Email)
                )
            {
                var roles = await _context.Roles
                    .Where(r => r.Name == "User")
                    .ToArrayAsync();

                var newUser = AddUserToDb(user, roles);

                return _mapper.Map<User>(newUser);
            }

            return null;
        }

        public async Task<User> AddAdministrator(UserRegister user)
        {
            if (!string.IsNullOrWhiteSpace(user.Password) &&
                !await _context.Users.AnyAsync(u =>
                    u.UserName == user.UserName || u.Email == user.Email)
                )
            {
                var roles = await _context.Roles
                    .Where(r => r.Name == "Administrator" || r.Name == "User")
                    .ToArrayAsync();

                var newUser = AddUserToDb(user, roles);

                return _mapper.Map<User>(newUser);
            }

            return null;
        }

        public async Task DeleteUser(int id)
        {
            var entity = await _context.Users.FindAsync(id);
            if (entity != null)
            {
                _context.Users.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                return _mapper.Map<User>(user);
            }
            return null;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                return _mapper.Map<User>(user);
            }
            return null;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            var user = await _context.Users
                .Include(u => u.Roles)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == username);
            if (user != null)
            {
                return _mapper.Map<User>(user);
            }

            return null;
        }

        public async Task<UserCredentials> GetUserCredentials(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                return _mapper.Map<UserCredentials>(user);
            }

            return null;
        }

        public async Task<IEnumerable<User>> GetUsers()
        {
            var users = await _context.Users.AsNoTracking().ToListAsync();
            return _mapper.Map<IEnumerable<User>>(users);
        }

        public async Task<User> UpdateUserDetails(int id, UserDetails user)
        {
            var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (userEntity != null)
            {
                userEntity.FirstName = user.FirstName;
                userEntity.LastName = user.LastName;
                userEntity.Email = user.Email;
                userEntity.Modified = DateTime.Now;

                await _context.SaveChangesAsync();

                return _mapper.Map<User>(userEntity);
            }

            return null;
        }

        public async Task<bool> UpdateUserPassword(int id, string password)
        {
            var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (userEntity != null)
            {
                userEntity.Password = Cryptography.CreateHash(password, userEntity.Salt);
                userEntity.Modified = DateTime.Now;
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        private async Task<Data.Entities.User> AddUserToDb(UserRegister user, Data.Entities.Role[] roles)
        {
            var passwordSalt = Cryptography.CreateSalt();
            var passwordHash = Cryptography.CreateHash(user.Password, passwordSalt); ;

            var entity = new Data.Entities.User()
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

            entity = _context.Users.Add(entity).Entity;
            await _context.SaveChangesAsync();

            return entity;
        }
    }
}
