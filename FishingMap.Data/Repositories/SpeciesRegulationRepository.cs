using FishingMap.Data.Context;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FishingMap.Data.Repositories
{
    public class SpeciesRegulationRepository : Repository<SpeciesRegulation>, ISpeciesRegulationRepository
    {
        public SpeciesRegulationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<SpeciesRegulation>> GetCandidatesForLocation(int locationId, IEnumerable<int> ancestorRegionIds)
        {
            var ancestorIds = ancestorRegionIds.ToList();

            return await _context.SpeciesRegulations
                .AsNoTracking()
                .Include(r => r.Locations)
                .Include(r => r.ProtectedPeriods)
                .Include(r => r.Region)
                .AsSplitQuery()
                .Where(r =>
                    r.Locations.Any(l => l.Id == locationId)
                    || (r.RegionId.HasValue && ancestorIds.Contains(r.RegionId.Value)))
                .ToListAsync();
        }
    }
}
