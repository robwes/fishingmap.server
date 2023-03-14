using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.Features;
using NetTopologySuite.Geometries;

namespace FishingMap.Domain.Data.Entities
{
    public class Location
    {
        public Location()
        {}

        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string LicenseInfo { get; set; }
        public string Rules { get; set; }
        public MultiPolygon Geometry { get; set; }
        [Required]
        public Point Position { get; set; }
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
    }
}
