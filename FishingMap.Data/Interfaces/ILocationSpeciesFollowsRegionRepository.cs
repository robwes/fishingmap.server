using FishingMap.Data.Entities;

namespace FishingMap.Data.Interfaces
{
    public interface ILocationSpeciesFollowsRegionRepository : IRepository<LocationSpeciesFollowsRegion>
    {
        // The species a water inherits its region's rules for. Resolution runs only for
        // these, so this is what separates "inherits" from "nobody has decided".
        Task<IReadOnlyList<int>> GetFollowedSpeciesIds(int locationId);
    }
}
