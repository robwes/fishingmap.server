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

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        
        public string? Rules { get; set; }
        public string? Geometry { get; set; }

        [Required]
        public GeoPoint Position { get; set; } = new GeoPoint();

        public GeoPoint? NavigationPosition { get; set; }
        public string? WebSite { get; set; }

        [FromForm]
        public IEnumerable<SpeciesDTO> Species { get; set; } = new List<SpeciesDTO>();

        public List<IFormFile> Images { get; set; } = new List<IFormFile>();

        [FromForm]
        public IEnumerable<PermitDTO> Permits { get; set; } = new List<PermitDTO>();
    }
}
