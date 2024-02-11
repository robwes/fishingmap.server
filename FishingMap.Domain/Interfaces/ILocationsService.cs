using FishingMap.Domain.DTO.Locations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FishingMap.Domain.Interfaces
{
    public interface ILocationsService
    {
        Task<LocationDTO> AddLocation(LocationAdd location);
        Task DeleteLocation(int id);
        Task<LocationDTO> GetLocation(int id);
        Task<IEnumerable<LocationSummary>> GetLocations(string search = "", List<int> speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null);
        Task<IEnumerable<LocationSummary>> GetLocationsSummary(string search = "", List<int> speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null);
        Task<IEnumerable<LocationMarker>> GetMarkers(string search = "", List<int> speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null);
        Task<LocationDTO> UpdateLocation(int id, LocationUpdate location);
    }
}
