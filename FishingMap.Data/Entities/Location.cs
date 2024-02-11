using System.ComponentModel.DataAnnotations;
using FishingMap.Data.Interfaces;
using NetTopologySuite.Geometries;

#nullable disable

namespace FishingMap.Data.Entities
{
    public class Location : IEntity
    {
        public Location()
        {}

        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        public string Description { get; set; }
        public string Rules { get; set; }
        public MultiPolygon Geometry { get; set; }

        [Required]
        public Point Position { get; set; }
        public Point NavigationPosition { get; set; }

        public double? AverageDepth { get; set; }
        public double? MaxDepth { get; set; }
        public double? Area { get; set; }
        public string WebSite { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public DateTime Modified { get; set; }

        public int? LocationOwnerId { get; set; }
        public virtual LocationOwner Owner { get; set; }

        public virtual ICollection<Species> Species { get; set; }
        public virtual ICollection<Image> Images { get; set; }
        public virtual ICollection<Permit> Permits { get; set; }
    }
}
