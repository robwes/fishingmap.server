using FishingMap.Common.Extensions;
using FishingMap.Data.Context;
using FishingMap.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Location = FishingMap.Data.Entities.Location;

namespace FishingMap.Data.Repositories
{
    public class LocationRepository : Repository<Location>, ILocationRepository
    {
        private readonly GeometryFactory _geometryFactory;
        public LocationRepository(ApplicationDbContext context, GeometryFactory geometryFactory)
            :base(context)
        {
            _geometryFactory = geometryFactory;
        }

        public async Task<List<Location>> FindLocations(string search = "", List<int>? speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null)
        {
            var query = _context.Locations
                .Include(l => l.Species.OrderBy(s => s.Name))
                .Include(l => l.Images)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(l => l.Name.Contains(search));
            }

            if (speciesIds?.Count > 0)
            {
                query = query.Where(l => l.Species.Any(s => speciesIds.Contains(s.Id)));
            }

            if (radius != null && orgLat != null && orgLng != null)
            {
                var origin = _geometryFactory.CreatePoint(orgLng.Value, orgLat.Value);
                query = query.Where(l => l.Position.IsWithinDistance(origin, radius.Value * 1000));
            }

            return await query.AsNoTracking().ToListAsync();
        }
    }
}
