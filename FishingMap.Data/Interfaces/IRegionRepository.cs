using FishingMap.Data.Entities;

namespace FishingMap.Data.Interfaces
{
    public interface IRegionRepository : IRepository<Region>
    {
        Task<int> GetRootRegionId();
        Task<IReadOnlyList<Region>> GetAncestry(int regionId);
    }
}
