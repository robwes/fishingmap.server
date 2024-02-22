using FishingMap.Data.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Data.Entities
{
    public class Species : IEntity
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime Created { get; set; }
        [Required]
        public DateTime Modified { get; set; }

        public virtual ICollection<Location> Locations { get; set; } = new HashSet<Location>();
        public virtual ICollection<Image> Images { get; set; } = new HashSet<Image>();
    }
}
