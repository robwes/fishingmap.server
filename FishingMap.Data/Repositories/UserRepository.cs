using FishingMap.Data.Context;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using Microsoft.Identity.Client;

namespace FishingMap.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetUserWithRoles(int id, bool noTracking = false)
        {
            return await this.GetById(
                id,
                [u => u.Roles.OrderBy(r => r.Name)],
                noTracking);
        }
    }
}
