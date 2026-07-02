using FishingMap.Domain.DTO.Geometries;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.DTO.Locations
{
    public class LocationGeometryPatch
    {
        [Required]
        public string Geometry { get; set; } = string.Empty;

        public GeoPoint? NavigationPosition { get; set; }
    }
}
