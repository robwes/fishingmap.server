using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using GeoAPI.Geometries;

namespace FishingMap.Domain.Data.Entities
{
    public class Location
    {
        public Location()
        {

        }


        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string FishingPermitInfo { get; set; }
        public string Rules { get; set; }
        public IPolygon Points { get; set; }
        [Required]
        public IPoint Position { get; set; }
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
        public virtual ICollection<LocationSpecies> LocationSpecies { get; set; }
    }
}
