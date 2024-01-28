using FishingMap.Common.Models.GeoObjects;
using FishingMap.Common.Models.SpeciesObjects;

namespace FishingMap.Common.Models.LocationObjects
{
    public class LocationMarker
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public GeoPoint Position { get; set; } = new GeoPoint();
        public IEnumerable<SpeciesIdName> Species { get; set; } = new List<SpeciesIdName>();
    }
}