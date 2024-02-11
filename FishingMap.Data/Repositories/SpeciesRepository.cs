using FishingMap.Data.Context;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using System.Linq.Expressions;

namespace FishingMap.Data.Repositories
{
    public class SpeciesRepository : Repository<Species>, ISpeciesRepository
    {
        public SpeciesRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Species?> GetSpeciesWithImages(int id, bool noTracking = false)
        {
            return await GetById(id, [s => s.Images], noTracking);
        }

        public async Task<IEnumerable<Species>> FindSpecies(string nameSearch = "")
        {
            Expression<Func<Species, bool>>? query = null;
            if (!string.IsNullOrEmpty(nameSearch))
            {
                query = s => s.Name.Contains(nameSearch);
            }

            return await GetAll(
                query,
                [s => s.Images],
                s => s.OrderBy(s => s.Name));
        }
    }
}
