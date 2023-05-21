using AutoMapper;
using FishingMap.Domain.Data.Context;
using FishingMap.Domain.Data.DTO;
using FishingMap.Domain.Interfaces;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using FishingMap.Domain.Extensions;

namespace FishingMap.Domain.Services
{
    public class LocationsService : ILocationsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly GeometryFactory _geometryFactory;
        private readonly IFileService _fileService;
        private readonly IFishingMapConfiguration _config;

        public LocationsService(
            ApplicationDbContext context,  
            IFileService fileService,
            IFishingMapConfiguration config,
            GeometryFactory geometryFactory,
            IMapper mapper)
        {
            _context = context;
            _fileService = fileService;
            _config = config;
            _geometryFactory = geometryFactory;
            _mapper = mapper;
        }

        public async Task<Data.DTO.Location> AddLocation(LocationUpdate location)
        {
            var entity = new Data.Entities.Location
            {
                Name = location.Name,
                Description = location.Description,
                Rules = location.Rules,
                WebSite = location.WebSite
            };

            if (location.Species != null)
            {
                var sIds = location.Species.Select(f => f.Id).Distinct();
                var species = await _context.Species.Where(s => sIds.Contains(s.Id)).ToListAsync();
                entity.Species = species;
            }           

            if (location.Permits != null)
            {
                var pIds = location.Permits.Select(f => f.Id).Distinct();
                var permits = await _context.Permits.Where(p => pIds.Contains(p.Id)).ToListAsync(); 
                entity.Permits = permits;
            }

            entity.Geometry = _geometryFactory.GeoJsonFeatureToMultiPolygon(location.Geometry);
            entity.Position = entity.Geometry.Centroid;
            entity.Area = entity.Geometry.Area;

            entity.Created = DateTime.Now;
            entity.Modified = DateTime.Now;
            entity = _context.Locations.Add(entity).Entity;
            await _context.SaveChangesAsync();

            if (location.Images?.Count > 0)
            {
                entity.Images = new List<Data.Entities.Image>();
                foreach (var image in location.Images)
                {
                    await AddLocationImage(entity, image);
                }
                await _context.SaveChangesAsync();
            }

            return _mapper.Map<Data.Entities.Location, Data.DTO.Location>(entity);
        }

        public async Task DeleteLocation(int id)
        {
            var location = await _context.Locations.Include(l => l.Images).FirstOrDefaultAsync(l => l.Id == id);
            if (location != null)
            {
                foreach (var image in location.Images)
                {
                    _context.Images.Remove(image);
                }

                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();

                await _fileService.DeleteFolder(
                    _config.GetPathToLocationsImageFolder(location.Id)
                );
            }
        }

        public async Task<Data.DTO.Location> GetLocation(int id)
        {
            var location = await _context.Locations
                .Include(l => l.Species)
                .Include(l => l.Permits)
                .Include(l => l.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id);
            if (location != null)
            {
                return _mapper.Map<Data.Entities.Location, Data.DTO.Location>(location);
            }
            return null;
        }

        public async Task<IEnumerable<Data.DTO.Location>> GetLocations(string search = "", List<int> speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null)
        {
            List<Data.Entities.Location> locations = await FindLocations(search, speciesIds, radius, orgLat, orgLng);

            return _mapper.Map<IEnumerable<Data.Entities.Location>, IEnumerable<Data.DTO.Location>>(locations);
        }

        public async Task<IEnumerable<LocationMarker>> GetMarkers(string search = "", List<int> speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null)
        {
            var locations = await FindLocations(search, speciesIds, radius, orgLat, orgLng);
            return _mapper.Map<IEnumerable<Data.Entities.Location>, IEnumerable<LocationMarker>>(locations);
        }

        public async Task<Data.DTO.Location> UpdateLocation(int id, LocationUpdate location)
        {
            var entity = await _context.Locations
                .Include(l => l.Species)
                .Include(l => l.Permits)
                .Include(l => l.Images)
                .FirstOrDefaultAsync(l => l.Id == id);
            
            if (entity != null)
            {
                entity.Name = location.Name;
                entity.Description = location.Description;
                entity.Rules = location.Rules;
                entity.WebSite = entity.WebSite;

                var polygon = _geometryFactory.GeoJsonFeatureToMultiPolygon(location.Geometry);
                if (!entity.Geometry.HasSameCoordinates(polygon))
                {
                    entity.Geometry = polygon;
                    entity.Position = entity.Geometry.Centroid;
                    entity.Area = polygon.Area;
                }             

                if (location.Species != null)
                {
                    var sIds = location.Species.Select(s => s.Id).Distinct();
                    var species = await _context.Species.Where(s => sIds.Contains(s.Id)).ToListAsync();
                    entity.Species = species;
                }
                else
                {
                    entity.Species.Clear();
                }

                if (location.Permits != null)
                {
                    var pIds = location.Permits.Select(s => s.Id).Distinct();
                    var permits = await _context.Permits.Where(p => pIds.Contains(p.Id)).ToListAsync();
                    entity.Permits = permits;
                }
                else
                {
                    entity.Permits.Clear();
                }

                if (location.Images?.Count > 0 && entity.Images?.Count > 0)
                {
                    var imagesInUpdateModel = location.Images?.Select(img => img.FileName);
                    var imagesInDb = entity.Images?.Select(img => img.Name);

                    var imagesToDelete = entity.Images?.Where(img => !imagesInUpdateModel.Contains(img.Name)).ToList();
                    foreach (var image in imagesToDelete)
                    {
                        DeleteLocationImage(entity, image);
                    }

                    var imagesToAdd = location.Images.Where(i => !imagesInDb.Contains(i.FileName));
                    foreach (var image in imagesToAdd)
                    {
                        await AddLocationImage(entity, image);
                    }
                }
                else if (location.Images?.Count > 0 && entity.Images?.Count == 0)
                {
                    foreach (var image in location.Images)
                    {
                        await AddLocationImage(entity, image);
                    }
                }
                else if (location.Images.IsNullOrEmpty())
                {
                    foreach (var image in entity.Images)
                    {
                        DeleteLocationImage(entity, image);
                    }
                }

                entity.Modified = DateTime.Now;
                await _context.SaveChangesAsync();

                return _mapper.Map<Data.Entities.Location, Data.DTO.Location>(entity);
            }

            return null;
        }

        private async Task<List<Data.Entities.Location>> FindLocations(string search = "", List<int> speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null)
        {
            var query = _context.Locations
                .Include(l => l.Species)
                .Include(l => l.Images)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(l => l.Name.Contains(search));
            }

            if (radius != null && orgLat != null && orgLng != null)
            {
                var origin = _geometryFactory.CreatePoint(orgLng.Value, orgLat.Value);
                query = query.Where(l => l.Position.IsWithinDistance(origin, radius.Value * 1000));
            }

            if (speciesIds?.Count > 0)
            {
                query = query.Where(l => l.Species.Any(s => speciesIds.Contains(s.Id)));
            }
            var locations = await query.OrderBy(l => l.Name).AsNoTracking().ToListAsync();
            return locations;
        }

        private void DeleteLocationImage(Data.Entities.Location location, Data.Entities.Image image)
        {
            location.Images.Remove(image);
            _context.Images.Remove(image);
            _fileService.DeleteFile(image.Path);
        }

        private async Task AddLocationImage(Data.Entities.Location location, IFormFile image)
        {
            var filePath = await _fileService.AddFile(
                image,
                Path.Combine("locations", location.Id.ToString())
            );
            var fileName = Path.GetFileName(filePath);

            location.Images.Add(new Data.Entities.Image
            {
                Name = fileName,
                Path = filePath,
                Created = DateTime.Now,
                Modified = DateTime.Now
            });
        }
    }
}
