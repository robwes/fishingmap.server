using FishingMap.Data.Entities;

namespace FishingMap.Domain.DTO.Regions
{
    public class RegionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public RegionType Type { get; set; }
        public int? ParentRegionId { get; set; }
    }
}
