using FishingMap.Domain.Data.DTO.LocationObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FishingMap.Domain.Interfaces
{
    public interface ILocationsService
    {
        Task<Location> AddLocation(LocationAdd location);
        Task<IEnumerable<LocationSummary>> GetLocations(string search = "", List<int> speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null);
        Task<IEnumerable<LocationSummary>> GetLocationsSummary(string search = "", List<int> speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null);
        Task<Location> GetLocation(int id);
        Task DeleteLocation(int id);
        Task<IEnumerable<LocationMarker>> GetMarkers(string search = "", List<int> speciesIds = null, double? radius = null, double? orgLat = null, double? orgLng = null);
        Task<Location> UpdateLocation(int id, LocationUpdate location);
    }
}
