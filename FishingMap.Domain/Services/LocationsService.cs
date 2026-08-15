using MapsterMapper;
using FishingMap.Common.Extensions;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.DTO.Images;
using FishingMap.Domain.DTO.Locations;
using FishingMap.Domain.Interfaces;
using FishingMap.Domain.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
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
        private readonly IRegulationsService _regulationsService;

        public LocationsService(
            IUnitOfWork unitOfWork,
            IFileService fileService,
            IFishingMapConfiguration config,
            GeometryFactory geometryFactory,
            IMapper mapper,
            IRegulationsService regulationsService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _config = config;
            _geometryFactory = geometryFactory;
            _mapper = mapper;
            _regulationsService = regulationsService;
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

            await AssignRegion(entity, location.RegionId);

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

            entity.Created = DateTime.UtcNow;
            entity.Modified = DateTime.UtcNow;
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
            if (location == null)
            {
                return null;
            }

            var dto = _mapper.Map<Location, LocationDTO>(location);
            dto.SpeciesRules = await _regulationsService.GetEffectiveRulesForLocation(id);
            dto.FollowsRegionSpeciesIds = await _regulationsService.GetFollowedSpeciesIds(id);
            return dto;
        }

        public async Task<IEnumerable<LocationSummary>> GetLocations(string search = "", List<int>? speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null)
        {
            var results = await _unitOfWork.Locations.FindLocationsWithDistance(search, speciesIds, radius, orgLat, orgLng);
            return results.Select(r =>
            {
                var dto = _mapper.Map<Location, LocationSummary>(r.Location);
                dto.Distance = r.DistanceKm;
                return dto;
            }).ToList();
        }

        public async Task<IEnumerable<LocationMarker>> GetMarkers(string search = "", List<int>? speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null)
        {
            var locations = await _unitOfWork.Locations.FindLocations(search, speciesIds, radius, orgLat, orgLng);
            return _mapper.Map<IEnumerable<Location>, IEnumerable<LocationMarker>>(locations);
        }

        public async Task<string> GetFeatures(string search = "", List<int>? speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null)
        {
            var locations = await _unitOfWork.Locations.FindLocations(search, speciesIds, radius, orgLat, orgLng);

            FeatureCollection features = new FeatureCollection();

            foreach (var location in locations)
            {
                var feature = new Feature() { 
                    Geometry = location.Position,
                    Attributes = new AttributesTable()
                };

                feature.Attributes.Add("id", location.Id);
                feature.Attributes.Add("name", location.Name);
                feature.Attributes.Add("description", location.Description);

                var species = location.Species.Select(s => new { id = s.Id, name = s.Name });
                feature.Attributes.Add("species", species);

                features.Add(feature);
            }

            var writer = new GeoJsonWriter();
            var geoJson = writer.Write(features);

            if (geoJson == null)
            {
                throw new InvalidOperationException("Error creating GeoJson");
            }

            return geoJson;

        }

        public async Task<IEnumerable<LocationSummary>> GetLocationsSummary(string search = "", List<int>? speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null)
        {
            var results = await _unitOfWork.Locations.FindLocationsWithDistance(search, speciesIds, radius, orgLat, orgLng);
            return results.Select(r =>
            {
                var dto = _mapper.Map<Location, LocationSummary>(r.Location);
                dto.Distance = r.DistanceKm;
                return dto;
            }).ToList();
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

            await AssignRegion(entity, location.RegionId);

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

            await PruneFollowsRegion(entity.Id, entity.Species?.Select(s => s.Id) ?? []);

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

            entity.Modified = DateTime.UtcNow;
            await _unitOfWork.SaveChanges();

            return _mapper.Map<Location, LocationDTO>(entity);
        }

        private async Task AssignRegion(Location entity, int? regionId)
        {
            if (!regionId.HasValue)
            {
                entity.RegionId = null;
                entity.Region = null;
                return;
            }

            var region = await _unitOfWork.Regions.GetById(regionId.Value);
            if (region == null)
            {
                throw new ArgumentException($"Region with id {regionId.Value} not found.");
            }

            entity.Region = region;
            entity.RegionId = region.Id;
        }

        private async Task<Image> AddLocationImage(Location location, IFormFile image)
        {
            ImageUpload.Validate(image);

            var filePath = await _fileService.AddFile(
                image,
                $"locations/{location.Id}"
            );
            var fileName = Path.GetFileName(filePath);

            if (location.Images == null)
            {
                location.Images = new List<Image>();
            }

            var newImage = new Image
            {
                Name = fileName,
                Path = filePath,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            };
            location.Images.Add(newImage);
            return newImage;
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

        public async Task<LocationDTO> UpdateLocationInfo(int id, LocationInfoPatch patch)
        {
            var entity = await _unitOfWork.Locations.GetLocationWithDetails(id);
            if (entity == null)
                throw new KeyNotFoundException($"Location with id {id} not found.");

            if (patch.Name.HasValue)
            {
                if (string.IsNullOrEmpty(patch.Name.Value))
                    throw new ArgumentException("Name cannot be empty.");
                entity.Name = patch.Name.Value;
            }
            if (patch.Description.HasValue) entity.Description = string.IsNullOrEmpty(patch.Description.Value) ? null : patch.Description.Value;
            if (patch.Rules.HasValue) entity.Rules = string.IsNullOrEmpty(patch.Rules.Value) ? null : patch.Rules.Value;
            if (patch.WebSite.HasValue) entity.WebSite = string.IsNullOrEmpty(patch.WebSite.Value) ? null : patch.WebSite.Value;
            if (patch.RegionId.HasValue) await AssignRegion(entity, patch.RegionId.Value);

            entity.Modified = DateTime.UtcNow;
            await _unitOfWork.SaveChanges();
            return _mapper.Map<Location, LocationDTO>(entity);
        }

        /// <summary>
        /// Drops "follows the region" decisions for species no longer listed at this water.
        ///
        /// Without it, removing a species and adding it back later would silently restore an
        /// inheritance nobody re-chose — the water would resume publishing rules on the
        /// strength of a decision made about a species it had since dropped.
        /// See decision 11 in robwes/fishingmap.web#13.
        /// </summary>
        /// <param name="locationId">The water whose species list just changed.</param>
        /// <param name="keptSpeciesIds">The species still listed there.</param>
        private async Task PruneFollowsRegion(int locationId, IEnumerable<int> keptSpeciesIds)
        {
            var kept = keptSpeciesIds.ToList();
            var rows = await _unitOfWork.LocationSpeciesFollowsRegions.GetAll(f => f.LocationId == locationId);

            foreach (var row in rows.Where(r => !kept.Contains(r.SpeciesId)))
            {
                _unitOfWork.LocationSpeciesFollowsRegions.Delete(row);
            }
        }

        public async Task<LocationDTO> UpdateLocationAssociations(int id, LocationAssociationsPatch patch)
        {
            var entity = await _unitOfWork.Locations.GetLocationWithDetails(id);
            if (entity == null)
                throw new KeyNotFoundException($"Location with id {id} not found.");

            var sIds = patch.Species.Select(s => s.Id).Distinct();
            var species = await _unitOfWork.Species.GetAll(s => sIds.Contains(s.Id));
            entity.Species = (ICollection<Species>)species;

            await PruneFollowsRegion(entity.Id, sIds);

            var pIds = patch.Permits.Select(p => p.Id).Distinct();
            var permits = await _unitOfWork.Permits.GetAll(p => pIds.Contains(p.Id));
            entity.Permits = (ICollection<Permit>)permits;

            entity.Modified = DateTime.UtcNow;
            await _unitOfWork.SaveChanges();
            return _mapper.Map<Location, LocationDTO>(entity);
        }

        public async Task<ImageDTO> AddImageToLocation(int id, IFormFile image)
        {
            var entity = await _unitOfWork.Locations.GetById(id, [l => l.Images]);
            if (entity == null)
                throw new KeyNotFoundException($"Location with id {id} not found.");

            var newImage = await AddLocationImage(entity, image);
            await _unitOfWork.SaveChanges();
            return _mapper.Map<Image, ImageDTO>(newImage);
        }

        public async Task RemoveImageFromLocation(int id, int imageId)
        {
            var entity = await _unitOfWork.Locations.GetById(id, [l => l.Images]);
            if (entity == null)
                throw new KeyNotFoundException($"Location with id {id} not found.");

            var image = entity.Images?.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
                throw new KeyNotFoundException($"Image with id {imageId} not found on location {id}.");

            await DeleteLocationImage(entity, image);
            await _unitOfWork.SaveChanges();
        }

        public async Task<LocationDTO> UpdateLocationGeometry(int id, LocationGeometryPatch patch)
        {
            var entity = await _unitOfWork.Locations.GetLocationWithDetails(id);
            if (entity == null)
                throw new KeyNotFoundException($"Location with id {id} not found.");

            MultiPolygon? geometry;
            try
            {
                geometry = _geometryFactory.GeoJsonFeatureToMultiPolygon(patch.Geometry);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid geometry");
            }
            if (geometry == null)
                throw new ArgumentException("Invalid geometry");

            entity.Geometry = geometry;
            entity.Position = entity.Geometry.Centroid;
            entity.Area = entity.Geometry.Area;

            if (patch.NavigationPosition != null)
                entity.NavigationPosition = _geometryFactory.CreatePoint(
                    patch.NavigationPosition.Longitude,
                    patch.NavigationPosition.Latitude);
            else
                entity.NavigationPosition = null;

            entity.Modified = DateTime.UtcNow;
            await _unitOfWork.SaveChanges();
            return _mapper.Map<Location, LocationDTO>(entity);
        }
    }
}
