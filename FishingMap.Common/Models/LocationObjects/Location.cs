using FishingMap.Common.Models.GeoObjects;
using FishingMap.Common.Models.ImageObjects;
using FishingMap.Common.Models.PermitObjects;
using FishingMap.Common.Models.SpeciesObjects;

namespace FishingMap.Common.Models.LocationObjects
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Rules { get; set; } = string.Empty;
        public string Geometry { get; set; } = string.Empty;
        public GeoPoint? Position { get; set; }
        public GeoPoint? NavigationPosition { get; set; }
        public string WebSite { get; set; } = string.Empty;       
        public IEnumerable<SpeciesIdName> Species { get; set; } = new List<SpeciesIdName>();    
        public IEnumerable<Permit> Permits { get; set; } = new List<Permit>();
        public IEnumerable<Image> Images { get; set; } = new List<Image>();
    }
}
