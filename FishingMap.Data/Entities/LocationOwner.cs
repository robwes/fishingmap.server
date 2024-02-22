using FishingMap.Data.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Data.Entities
{
    public class LocationOwner : IEntity
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? StreetAddress { get; set; }
        public string? ZipCode { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        public string? WebSite { get; set; }
        public string? Email { get; set; }

        [Required]
        public DateTime Created { get; set; }
        [Required]
        public DateTime Modified { get; set; }
        public virtual ICollection<Location> Locations { get; set; } = new HashSet<Location>();
    }
}
