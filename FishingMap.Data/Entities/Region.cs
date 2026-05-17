using FishingMap.Data.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Data.Entities
{
    public class Region : IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public RegionType Type { get; set; }

        public int? ParentRegionId { get; set; }
        public virtual Region? Parent { get; set; }
        public virtual ICollection<Region> Children { get; set; } = new HashSet<Region>();

        public virtual ICollection<Location> Locations { get; set; } = new HashSet<Location>();
        public virtual ICollection<SpeciesRegulation> Regulations { get; set; } = new HashSet<SpeciesRegulation>();

        [Required]
        public DateTime Created { get; set; }
        [Required]
        public DateTime Modified { get; set; }
    }
}
