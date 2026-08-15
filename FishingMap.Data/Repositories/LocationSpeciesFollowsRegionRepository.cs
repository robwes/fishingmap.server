using FishingMap.Data.Context;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FishingMap.Data.Repositories
{
    public class LocationSpeciesFollowsRegionRepository
        : Repository<LocationSpeciesFollowsRegion>, ILocationSpeciesFollowsRegionRepository
    {
        public LocationSpeciesFollowsRegionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<int>> GetFollowedSpeciesIds(int locationId)
        {
            return await _context.LocationSpeciesFollowsRegions
                .AsNoTracking()
                .Where(f => f.LocationId == locationId)
                .Select(f => f.SpeciesId)
                .ToListAsync();
        }
    }
}
