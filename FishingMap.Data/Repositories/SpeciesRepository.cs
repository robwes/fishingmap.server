using FishingMap.Data.Context;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;

namespace FishingMap.Data.Repositories
{
    public class SpeciesRepository : Repository<Species>, ISpeciesRepository
    {
        public SpeciesRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
