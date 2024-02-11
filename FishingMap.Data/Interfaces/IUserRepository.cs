using FishingMap.Data.Entities;

namespace FishingMap.Data.Interfaces
{
    public interface IUserRepository : IRepository<User> 
    {
        Task<User?> GetUserWithRoles(int id, bool noTracking = false);
    }
}
