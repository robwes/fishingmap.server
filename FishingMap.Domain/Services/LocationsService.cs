using AutoMapper;
using FishingMap.Domain.Data.Context;
using FishingMap.Domain.Data.DTO;
using FishingMap.Domain.Data.Extensions;
using FishingMap.Domain.Interfaces;
using GeoAPI.CoordinateSystems;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Services
{
    public class LocationsService : ILocationsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly GeoAPI.Geometries.IGeometryFactory _geometryFactory;

        public LocationsService(ApplicationDbContext context, IMapper mapper, GeoAPI.Geometries.IGeometryFactory geometryFactory)
        {
            _context = context;
            _mapper = mapper;
            _geometryFactory = geometryFactory;
        }

        public async Task<Location> AddLocation(Location location)
        {
            var entity = new Data.Entities.Location
            {
                Name = location.Name,
                Description = location.Description,
                FishingPermitInfo = location.FishingPermitInfo,
                Rules = location.Rules,
                WebSite = location.WebSite
            };

            if (location.Species != null)
            {
                var sIds = location.Species.Select(f => f.Id).Distinct();
                var species = await _context.Species.Where(s => sIds.Contains(s.Id)).ToListAsync();
                var lsList = new List<Data.Entities.LocationSpecies>();
                species.ForEach(s =>
                    lsList.Add(new Data.Entities.LocationSpecies() { LocationId = location.Id, SpeciesId = s.Id })
                );
                entity.LocationSpecies = lsList;
            }

            entity.Points = _geometryFactory.GeoJsonFeatureToPolygon(location.Points);
            entity.Position = entity.Points.Centroid;
            entity.Area = entity.Points.Area;

            entity.Created = DateTime.Now;
            entity.Modified = DateTime.Now;
            entity = _context.Locations.Add(entity).Entity;
            await _context.SaveChangesAsync();

            return _mapper.Map<Data.Entities.Location, Location>(entity);
        }

        public async Task DeleteLocation(int id)
        {
            var location = await _context.Locations.FirstOrDefaultAsync(l => l.Id == id);
            if (location != null)
            {
                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Location> GetLocation(int id)
        {
            var location = await _context.Locations.Include(l => l.LocationSpecies).ThenInclude(ls => ls.Species).AsNoTracking().FirstOrDefaultAsync(l => l.Id == id);
            if (location != null)
            {
                return _mapper.Map<Data.Entities.Location, Location>(location);
            }
            return null;
        }

        public async Task<IEnumerable<Location>> GetLocations(string search = "", List<int> speciesIds = null, double? inRange = null, GeoPoint fromPosition = null)
        {
            List<Data.Entities.Location> locations = await FindLocations(search, speciesIds, inRange, fromPosition);

            return _mapper.Map<IEnumerable<Data.Entities.Location>, IEnumerable<Location>>(locations);
        }

        public async Task<IEnumerable<LocationMarker>> GetMarkers(string search = "", List<int> speciesIds = null, double? inRange = null, GeoPoint fromPosition = null)
        {
            var locations = await FindLocations(search, speciesIds, inRange, fromPosition);
            return _mapper.Map<IEnumerable<Data.Entities.Location>, IEnumerable<LocationMarker>>(locations);
        }

        public async Task<Location> UpdateLocation(Location location)
        {
            var entity = await _context.Locations.Include(l => l.LocationSpecies).ThenInclude(ls => ls.Species).FirstOrDefaultAsync(l => l.Id == location.Id);
            if (entity != null)
            {
                entity.Name = location.Name;
                entity.Description = location.Description;
                entity.FishingPermitInfo = location.FishingPermitInfo;
                entity.Rules = location.Rules;
                entity.WebSite = entity.WebSite;

                var polygon = _geometryFactory.GeoJsonFeatureToPolygon(location.Points);
                if (!entity.Points.HasSameCoordinates(polygon))
                {
                    entity.Points = polygon;
                    entity.Position = entity.Points.Centroid;
                    entity.Area = polygon.Area;
                }             

                if (location.Species != null)
                {
                    var sIds = location.Species.Select(s => s.Id).Distinct();
                    var species = await _context.Species.Where(s => sIds.Contains(s.Id)).ToListAsync();
                    var lsList = new List<Data.Entities.LocationSpecies>();
                    species.ForEach(s =>
                        lsList.Add(new Data.Entities.LocationSpecies() { LocationId = location.Id, SpeciesId = s.Id })
                    );
                    entity.LocationSpecies = lsList;
                }

                entity.Modified = DateTime.Now;
                await _context.SaveChangesAsync();

                return _mapper.Map<Data.Entities.Location, Location>(entity);
            }

            return null;
        }

        private async Task<List<Data.Entities.Location>> FindLocations(string search, List<int> speciesIds, double? inRange = null, GeoPoint fromPosition = null)
        {
            var query = _context.Locations.Include(l => l.LocationSpecies).ThenInclude(ls => ls.Species).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l => l.Name.StartsWith(search));
            }

            if (speciesIds?.Count > 0)
            {
                query = query.Where(l => l.LocationSpecies.Any(s => speciesIds.Contains(s.SpeciesId)));
            }
            var locations = await query.OrderBy(l => l.Name).AsNoTracking().ToListAsync();
            return locations;
        }
    }
}
