using FishingMap.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.DTO.Regions
{
    public class RegionAdd
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public RegionType Type { get; set; }

        public int? ParentRegionId { get; set; }
    }
}
