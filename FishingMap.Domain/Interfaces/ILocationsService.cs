using FishingMap.Domain.Data.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Interfaces
{
    public interface ILocationsService
    {
        Task<Location> AddLocation(Location location);
        Task<IEnumerable<Location>> GetLocations(string search = "", List<int> speciesIds = null, double? inRange = null, GeoPoint fromPosition = null);
        Task<Location> GetLocation(int id);
        Task DeleteLocation(int id);
        Task<IEnumerable<LocationMarker>> GetMarkers(string search = "", List<int> speciesIds = null, double? inRange = null, GeoPoint fromPosition = null);
        Task<Location> UpdateLocation(Location location);
    }
}
