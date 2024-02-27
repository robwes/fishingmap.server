using FishingMap.Domain.DTO.Geometries;
using FishingMap.Domain.DTO.Permits;
using FishingMap.Domain.DTO.Species;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.DTO.Locations
{
    public class LocationUpdate
    {
        [Required]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(5000)]
        public string? Description { get; set; }

        [StringLength(5000)]
        public string? Rules { get; set; }

        [Required]
        public string Geometry { get; set; } = string.Empty;

        [Required]
        public GeoPoint Position { get; set; } = new GeoPoint();

        public GeoPoint? NavigationPosition { get; set; }

        [StringLength(255)]
        public string? WebSite { get; set; }

        [FromForm]
        public IEnumerable<SpeciesDTO> Species { get; set; } = new List<SpeciesDTO>();

        public List<IFormFile> Images { get; set; } = new List<IFormFile>();

        [FromForm]
        public IEnumerable<PermitDTO> Permits { get; set; } = new List<PermitDTO>();
    }
}
