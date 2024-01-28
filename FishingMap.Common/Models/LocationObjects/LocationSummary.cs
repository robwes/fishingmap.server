using FishingMap.Common.Models.GeoObjects;
using FishingMap.Common.Models.ImageObjects;
using FishingMap.Common.Models.SpeciesObjects;

namespace FishingMap.Common.Models.LocationObjects
{
    public class LocationSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public GeoPoint Position { get; set; } = null!;
        public IEnumerable<SpeciesIdName> Species { get; set; } = new List<SpeciesIdName>();
        public IEnumerable<Image> Images { get; set; } = new List<Image>();
    }
}
