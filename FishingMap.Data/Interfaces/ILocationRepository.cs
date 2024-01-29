using FishingMap.Data.Entities;

namespace FishingMap.Data.Interfaces
{
    public interface ILocationRepository : IRepository<Location>
    {
        Task<List<Location>> FindLocations(
            string search = "", 
            List<int>? speciesIds = null, 
            double? radius = null, 
            double? orgLat = null, 
            double? orgLng = null);

        Task<Location?> GetLocationWithDetails(int id, bool noTracking = false);
    }
}
