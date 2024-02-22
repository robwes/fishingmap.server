using FishingMap.Domain.DTO.Geometries;
using FishingMap.Domain.DTO.Images;
using FishingMap.Domain.DTO.Species;

namespace FishingMap.Domain.DTO.Locations
{
    public class LocationSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public GeoPoint Position { get; set; } = new GeoPoint();
        public IEnumerable<SpeciesIdName> Species { get; set; } = new List<SpeciesIdName>();
        public IEnumerable<ImageDTO> Images { get; set; } = new List<ImageDTO>();
    }
}
