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

        public async Task<List<Location>> FindLocations(string nameSearch = "", List<int>? speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null)
        {
            var origin = (orgLat != null && orgLng != null)
                ? _geometryFactory.CreatePoint(orgLng.Value, orgLat.Value)
                : null;

            return await BuildFilteredQuery(nameSearch, speciesIds, radius, origin)
                .OrderBy(l => l.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<(Location Location, double? DistanceKm)>> FindLocationsWithDistance(string nameSearch = "", List<int>? speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null)
        {
            var origin = (orgLat != null && orgLng != null)
                ? _geometryFactory.CreatePoint(orgLng.Value, orgLat.Value)
                : null;

            var query = BuildFilteredQuery(nameSearch, speciesIds, radius, origin);

            if (origin != null)
            {
                var rows = await query
                    .Select(l => new { Location = l, Meters = l.Position.Distance(origin) })
                    .OrderBy(x => x.Meters)
                    .AsNoTracking()
                    .ToListAsync();

                return rows
                    .Select(x => (x.Location, (double?)Math.Round(x.Meters / 1000.0, 2)))
                    .ToList();
            }

            var locations = await query
                .OrderBy(l => l.Name)
                .AsNoTracking()
                .ToListAsync();

            return locations.Select(l => (l, (double?)null)).ToList();
        }

        private IQueryable<Location> BuildFilteredQuery(string nameSearch, List<int>? speciesIds, double? radius, Point? origin)
        {
            var query = _context.Locations
                .Include(l => l.Species.OrderBy(s => s.Name))
                .Include(l => l.Images)
                .AsSplitQuery()
                .AsQueryable();

            if (!string.IsNullOrEmpty(nameSearch))
            {
                query = query.Where(l => l.Name.Contains(nameSearch));
            }

            if (speciesIds?.Count > 0)
            {
                query = query.Where(l => l.Species.Any(s => speciesIds.Contains(s.Id)));
            }

            if (radius != null && origin != null)
            {
                query = query.Where(l => l.Position.IsWithinDistance(origin, radius.Value * 1000));
            }

            return query;
        }

        public async Task<Location?> GetLocationWithDetails(int id, bool noTracking = false)
        {
            var query = _context.Locations
                .Include(l => l.Species.OrderBy(s => s.Name))
                .Include(l => l.Permits.OrderBy(p => p.Name))
                .Include(l => l.Images)
                .Include(l => l.Region)
                .AsSplitQuery();

            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(l => l.Id == id);
        }
    }
}
