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

        public async Task<IReadOnlyList<SpeciesRegulation>> GetRegionRulesForSpecies(int speciesId)
        {
            // Region-scoped only, and the filter is here rather than in the caller so the
            // location-scoped rows and their Locations collections are never loaded. The
            // number of regions is bounded by the tree; the number of waters is not.
            return await _context.SpeciesRegulations
                .AsNoTracking()
                .Include(r => r.ProtectedPeriods)
                .Include(r => r.Region)
                .AsSplitQuery()
                .Where(r => r.SpeciesId == speciesId && r.RegionId != null)
                .ToListAsync();
        }
    }
}
