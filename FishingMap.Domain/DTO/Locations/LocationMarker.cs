using FishingMap.Domain.DTO.Geometries;
using FishingMap.Domain.DTO.Species;

namespace FishingMap.Domain.DTO.Locations
{
    public class LocationMarker
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public GeoPoint Position { get; set; } = new GeoPoint();
        public IEnumerable<SpeciesIdName> Species { get; set; } = new List<SpeciesIdName>();
    }
}