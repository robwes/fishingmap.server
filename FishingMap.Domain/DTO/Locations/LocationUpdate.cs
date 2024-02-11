using FishingMap.Domain.DTO.Geometries;
using FishingMap.Domain.DTO.Permits;
using FishingMap.Domain.DTO.Species;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace FishingMap.Domain.DTO.Locations
{
    public class LocationUpdate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<IFormFile> Images { get; set; }
        public string LicenseInfo { get; set; }
        public string Rules { get; set; }
        public string Geometry { get; set; }
        public GeoPoint Position { get; set; }
        public GeoPoint NavigationPosition { get; set; }
        public string WebSite { get; set; }

        [FromForm]
        public IEnumerable<SpeciesDTO> Species { get; set; }

        [FromForm]
        public IEnumerable<PermitDTO> Permits { get; set; }
    }
}
