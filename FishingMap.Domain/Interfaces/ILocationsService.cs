using FishingMap.Domain.DTO.Images;
using FishingMap.Domain.DTO.Locations;
using Microsoft.AspNetCore.Http;

namespace FishingMap.Domain.Interfaces
{
    public interface ILocationsService
    {
        Task<LocationDTO> AddLocation(LocationAdd location);
        Task DeleteLocation(int id);
        Task<LocationDTO?> GetLocation(int id);
        Task<IEnumerable<LocationSummary>> GetLocations(string search = "", List<int>? speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null);
        Task<IEnumerable<LocationSummary>> GetLocationsSummary(string search = "", List<int>? speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null);
        Task<IEnumerable<LocationMarker>> GetMarkers(string search = "", List<int>? speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null);
        Task<string> GetFeatures(string search = "", List<int>? speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null);
        Task<LocationDTO> UpdateLocation(int id, LocationUpdate location);
        Task<LocationDTO> UpdateLocationInfo(int id, LocationInfoPatch patch);
        Task<LocationDTO> UpdateLocationAssociations(int id, LocationAssociationsPatch patch);
        Task<ImageDTO> AddImageToLocation(int id, IFormFile image);
        Task RemoveImageFromLocation(int id, int imageId);
        Task<LocationDTO> UpdateLocationGeometry(int id, LocationGeometryPatch patch);
    }
}
