using FishingMap.Data.Context;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;

namespace FishingMap.Data.Repositories
{
    public class LocationOwnerRepository : Repository<LocationOwner>, ILocationOwnerRepository
    {
        public LocationOwnerRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
