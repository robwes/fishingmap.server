using FishingMap.Common.Utils;
using FishingMap.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FishingMap.Data.Context
{
    public class DbInitializer
    {
        private readonly ApplicationDbContext _context;

        public DbInitializer(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task InitializeAsync()
        {
            _context.Database.EnsureCreated();

            var now = DateTime.UtcNow;

            if (!_context.Regions.Any(r => r.Type == RegionType.Root))
            {
                _context.Regions.Add(new Region
                {
                    Name = "Finland",
                    Type = RegionType.Root,
                    ParentRegionId = null,
                    Created = now,
                    Modified = now
                });
                await _context.SaveChangesAsync();
            }

            if (_context.Users.Any())
            {
                return;
            }

            var passwordSalt = Cryptography.CreateSalt();
            var passwordHash = Cryptography.CreateHash("admin12", passwordSalt);

            // Reuse existing roles if present — Role.Name has a unique index
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Administrator")
                ?? new Role { Name = "Administrator" };
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User")
                ?? new Role { Name = "User" };

            var adminUser = new User
            {
                FirstName = "Lord Admin",
                LastName = "First of His Name",
                Email = "admin@fishingmap.se",
                UserName = "admin",
                Password = passwordHash,
                Salt = passwordSalt,
                Roles = new List<Role> { adminRole, userRole },
                Created = now,
                Modified = now
            };

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
        }
    }

}
