using FishingMap.Data.Context;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using System.Linq.Expressions;

namespace FishingMap.Data.Repositories
{
    public class PermitRepository : Repository<Permit>, IPermitRepository
    {
        public PermitRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Permit>> FindPermits(string nameSearch = "")
        {
            Expression<Func<Permit, bool>>? query = null;
            if (!string.IsNullOrEmpty(nameSearch))
            {
                query = p => p.Name.Contains(nameSearch);
            }

            return await GetAll(
                query,
                orderBy: p => p.OrderBy(p => p.Name));
        }
    }
}
