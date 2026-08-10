using FishingMap.Data.Context;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FishingMap.Data.Repositories
{
    public class RegionRepository : Repository<Region>, IRegionRepository
    {
        public RegionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<int> GetRootRegionId()
        {
            var national = await _context.Regions
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Type == RegionType.Root)
                ?? throw new InvalidOperationException("Root region row is missing. DbInitializer must seed it.");

            return national.Id;
        }

        public async Task<IReadOnlyList<Region>> GetAncestry(int regionId)
        {
            // Loop loads each ancestor with one query each. Acceptable: the chain is at most a few rows
            // (Location → kalatalousalue → ELY → National) and ancestry is read-only and cacheable.
            var chain = new List<Region>();
            var visited = new HashSet<int>();
            var current = await _context.Regions
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == regionId);

            while (current != null)
            {
                if (!visited.Add(current.Id))
                {
                    throw new InvalidOperationException(
                        $"Region hierarchy contains a cycle involving region {current.Id}.");
                }

                chain.Add(current);
                if (current.ParentRegionId is null)
                {
                    break;
                }

                var parentId = current.ParentRegionId.Value;
                current = await _context.Regions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == parentId);
            }

            return chain;
        }
    }
}
