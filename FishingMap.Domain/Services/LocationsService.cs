using AutoMapper;
using FishingMap.Common.Extensions;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.DTO.Locations;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Location = FishingMap.Data.Entities.Location;

namespace FishingMap.Domain.Services
{
    public class LocationsService : ILocationsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly GeometryFactory _geometryFactory;
        private readonly IFileService _fileService;
        private readonly IFishingMapConfiguration _config;

        public LocationsService(
            IUnitOfWork unitOfWork,  
            IFileService fileService,
            IFishingMapConfiguration config,
            GeometryFactory geometryFactory,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _config = config;
            _geometryFactory = geometryFactory;
            _mapper = mapper;
        }

        public async Task<LocationDTO> AddLocation(LocationAdd location)
        {
            var entity = new Location
            {
                Name = location.Name,
                Description = location.Description,
                Rules = location.Rules,
                WebSite = location.WebSite
            };

            if (location.Species != null)
            {
                var sIds = location.Species.Select(f => f.Id).Distinct();
                var species = await _unitOfWork.Species.GetAll(s => sIds.Contains(s.Id));
                entity.Species = (ICollection<Species>)species;
            }           

            if (location.Permits != null)
            {
                var pIds = location.Permits.Select(f => f.Id).Distinct();
                var permits = await _unitOfWork.Permits.GetAll(p => pIds.Contains(p.Id));               
                entity.Permits = (ICollection<Permit>)permits;
            }

            var geometry = _geometryFactory.GeoJsonFeatureToMultiPolygon(location.Geometry);
            if (geometry == null)
            {
                throw new ArgumentException("Invalid geometry");
            }

            entity.Geometry = geometry;
            entity.Position = entity.Geometry.Centroid;
            entity.Area = entity.Geometry.Area;

            if (location.NavigationPosition != null)
            {
                entity.NavigationPosition = _geometryFactory.CreatePoint(
                    location.NavigationPosition.Longitude,
                    location.NavigationPosition.Latitude
                );
            }
            else
            {
                entity.NavigationPosition = null;
            }

            entity.Created = DateTime.Now;
            entity.Modified = DateTime.Now;
            entity = _unitOfWork.Locations.Add(entity);
            await _unitOfWork.SaveChanges();

            if (location.Images?.Count > 0)
            {
                entity.Images = new List<Image>();
                foreach (var image in location.Images)
                {
                    await AddLocationImage(entity, image);
                }
                await _unitOfWork.SaveChanges();
            }

            return _mapper.Map<Location, LocationDTO>(entity);
        }

        public async Task DeleteLocation(int id)
        {
            var location = await _unitOfWork.Locations.GetById(id, [l => l.Images]);
            if (location != null)
            {
                foreach (var image in location.Images)
                {
                    _unitOfWork.Images.Delete(image);
                }

                _unitOfWork.Locations.Delete(location);
                await _unitOfWork.SaveChanges();

                await _fileService.DeleteFolder(
                    _config.GetPathToLocationsImageFolder(location.Id)
                );
            }
        }

        public async Task<LocationDTO?> GetLocation(int id)
        {
            var location = await _unitOfWork.Locations.GetLocationWithDetails(id, true);

            if (location != null)
            {
                return _mapper.Map<Location, LocationDTO>(location);
            }
            return null;
        }

        public async Task<IEnumerable<LocationSummary>> GetLocations(string search = "", List<int>? speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null)
        {
            var locations = await _unitOfWork.Locations.FindLocations(search, speciesIds, radius, orgLat, orgLng);
            return _mapper.Map<IEnumerable<Location>, IEnumerable<LocationSummary>>(locations);
        }

        public async Task<IEnumerable<LocationMarker>> GetMarkers(string search = "", List<int>? speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null)
        {
            var locations = await _unitOfWork.Locations.FindLocations(search, speciesIds, radius, orgLat, orgLng);
            return _mapper.Map<IEnumerable<Location>, IEnumerable<LocationMarker>>(locations);
        }

        public async Task<IEnumerable<LocationSummary>> GetLocationsSummary(string search = "", List<int>? speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null)
        {
            var locations = await _unitOfWork.Locations.FindLocations(search, speciesIds, radius, orgLat, orgLng);
            return _mapper.Map<IEnumerable<Location>, IEnumerable<LocationSummary>>(locations);
        }

        public async Task<LocationDTO> UpdateLocation(int id, LocationUpdate location)
        {
            var entity = await _unitOfWork.Locations.GetLocationWithDetails(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Location with id {id} not found.");
            }

            entity.Name = location.Name;
            entity.Description = location.Description;
            entity.Rules = location.Rules;
            entity.WebSite = location.WebSite;

            var geometry = _geometryFactory.GeoJsonFeatureToMultiPolygon(location.Geometry);
            if (geometry == null)
            {
                throw new ArgumentException("Invalid geometry");
            }
                
            entity.Geometry = geometry;
            entity.Position = entity.Geometry.Centroid;
            entity.Area = entity.Geometry.Area;

            if (location.NavigationPosition != null)
            {
                entity.NavigationPosition = _geometryFactory.CreatePoint(
                    location.NavigationPosition.Longitude,
                    location.NavigationPosition.Latitude
                );
            }
            else
            {
                entity.NavigationPosition = null;
            }

            if (location.Species != null)
            {
                var sIds = location.Species.Select(s => s.Id).Distinct();
                var species = await _unitOfWork.Species.GetAll(s => sIds.Contains(s.Id));
                entity.Species = (ICollection<Species>)species;
            }
            else
            {
                entity.Species?.Clear();
            }

            if (location.Permits != null)
            {
                var pIds = location.Permits.Select(s => s.Id).Distinct();
                var permits = await _unitOfWork.Permits.GetAll(p => pIds.Contains(p.Id));
                entity.Permits = (ICollection<Permit>)permits;
            }
            else
            {
                entity.Permits?.Clear();
            }

            await UpdateLocationsImages(entity, location);

            entity.Modified = DateTime.Now;
            await _unitOfWork.SaveChanges();

            return _mapper.Map<Location, LocationDTO>(entity);
        }

        private async Task AddLocationImage(Location location, IFormFile image)
        {
            var filePath = await _fileService.AddFile(
                image,
                $"locations/{location.Id}"
            );
            var fileName = Path.GetFileName(filePath);

            if (location.Images == null)
            {
                location.Images = new List<Image>();
            }

            location.Images.Add(new Image
            {
                Name = fileName,
                Path = filePath,
                Created = DateTime.Now,
                Modified = DateTime.Now
            });
        }

        private async Task DeleteLocationImage(Location location, Image image)
        {
            location.Images.Remove(image);
            _unitOfWork.Images.Delete(image);
            await _fileService.DeleteFile(image.Path);
        }

        

        private async Task UpdateLocationsImages(Location locationEntity, LocationUpdate locationUpdate)
        {
            if (!locationEntity.Images.IsNullOrEmpty())
            {
                // Get the list of file names of the images in the update model
                var imagesInUpdateModel = locationUpdate.Images?.Select(img => img.FileName) ?? new List<string>();
                // Find the images in the location entity that are not in the update model
                var imagesToDelete = locationEntity.Images.Where(img => !imagesInUpdateModel.Contains(img.Name)).ToList();

                foreach (var image in imagesToDelete)
                {
                    await DeleteLocationImage(locationEntity, image);
                }
            }

            if (!locationUpdate.Images!.IsNullOrEmpty())
            {
                // Get the list of file names of the images in the location entity
                var imagesInEntityModel = locationEntity.Images?.Select(img => img.Name) ?? new List<string>();
                // Find the images in the update model that are not in the location entity
                var imagesToAdd = locationUpdate.Images!.Where(i => !imagesInEntityModel.Contains(i.FileName)).ToList();

                foreach (var image in imagesToAdd)
                {
                    await AddLocationImage(locationEntity, image);
                }
            }
        }
    }
}
