using FishingMap.Data.Interfaces;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace FishingMap.Data.Entities
{
    public class Permit : IEntity
    {
        public Permit() {}

        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Url { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public DateTime Modified { get; set; }

        public virtual ICollection<Location> Locations { get; set; }
    }
}
