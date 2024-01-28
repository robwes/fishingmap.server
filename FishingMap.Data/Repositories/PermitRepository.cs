using FishingMap.Data.Context;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;

namespace FishingMap.Data.Repositories
{
    public class PermitRepository : Repository<Permit>, IPermitRepository
    {
        public PermitRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
