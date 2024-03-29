﻿using System.ComponentModel.DataAnnotations;
using FishingMap.Data.Interfaces;
using NetTopologySuite.Geometries;

namespace FishingMap.Data.Entities
{
    public class Location : IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(5000)]
        public string? Description { get; set; }

        [MaxLength(5000)]
        public string? Rules { get; set; }

        [Required]
        public MultiPolygon Geometry { get; set; } = new MultiPolygon([]) { SRID = 4326};

        [Required]
        public Point Position { get; set; } = new Point(0, 0) { SRID = 4326 };
        public Point? NavigationPosition { get; set; }

        public double? AverageDepth { get; set; }
        public double? MaxDepth { get; set; }
        public double? Area { get; set; }

        [MaxLength(255)]
        public string? WebSite { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public DateTime Modified { get; set; }

        public int? LocationOwnerId { get; set; }
        public virtual LocationOwner? Owner { get; set; }

        public virtual ICollection<Species> Species { get; set; } = new HashSet<Species>();
        public virtual ICollection<Image> Images { get; set; } = new HashSet<Image>();
        public virtual ICollection<Permit> Permits { get; set; } = new HashSet<Permit>();
    }
}
