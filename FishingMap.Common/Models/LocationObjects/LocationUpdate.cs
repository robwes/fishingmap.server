using FishingMap.Common.Models.GeoObjects;
using FishingMap.Common.Models.PermitObjects;
using FishingMap.Common.Models.SpeciesObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FishingMap.Common.Models.LocationObjects
{
    public class LocationUpdate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public string LicenseInfo { get; set; } = string.Empty;
        public string Rules { get; set; } = string.Empty;
        public string Geometry { get; set; } = string.Empty;
        public GeoPoint Position { get; set; } = null!;
        public GeoPoint? NavigationPosition { get; set; }
        public string WebSite { get; set; } = string.Empty;

        [FromForm]
        public IEnumerable<Species>? Species { get; set; }

        [FromForm]
        public IEnumerable<Permit>? Permits { get; set; }
    }
}
