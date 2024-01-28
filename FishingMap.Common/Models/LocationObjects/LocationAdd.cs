using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FishingMap.Common.Models.SpeciesObjects;
using FishingMap.Common.Models.PermitObjects;
using FishingMap.Common.Models.GeoObjects;

namespace FishingMap.Common.Models.LocationObjects
{
    public class LocationAdd
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;        
        public string Description { get; set; } = string.Empty;
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
        public string LicenseInfo { get; set; } = string.Empty;
        public string Rules { get; set; } = string.Empty;
        public string Geometry { get; set; } = string.Empty;
        public GeoPoint Position { get; set; } = new GeoPoint();
        public GeoPoint? NavigationPosition { get; set; }
        public string WebSite { get; set; } = string.Empty;

        [FromForm]
        public IEnumerable<Species> Species { get; set; } = new List<Species>();

        [FromForm]
        public IEnumerable<Permit> Permits { get; set; } = new List<Permit>();
    }
}
