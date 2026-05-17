using FishingMap.Domain.DTO.Regions;

namespace FishingMap.Domain.Interfaces
{
    public interface IRegionsService
    {
        Task<IEnumerable<RegionDTO>> GetRegions();
        Task<RegionDTO?> GetRegion(int id);
        Task<RegionDTO> AddRegion(RegionAdd region);
        Task<RegionDTO> UpdateRegion(int id, RegionUpdate region);
        Task DeleteRegion(int id);
    }
}
