using FishingMap.Common.Utils;
using FishingMap.Data.Entities;

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

            // Look for any users.
            if (_context.Users.Any())
            {
                return;   // DB has been seeded
            }

            var passwordSalt = Cryptography.CreateSalt();
            var passwordHash = Cryptography.CreateHash("admin12", passwordSalt);
            var now = DateTime.Now;

            var adminRole = new Role { Id = 1, Name = "Administrator" };
            var userRole = new Role { Id = 2, Name = "User" };

            var adminUser = new User
            {
                Id = 1,
                FirstName = "Lord Admin",
                LastName = "First of His Name",
                Email = "admin@fishingmap.se",
                UserName = "admin",
                Password = passwordHash,
                Salt = passwordSalt,
                Created = now,
                Modified = now
            };

            _context.Roles.AddRange(adminRole, userRole);
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
        }
    }

}
