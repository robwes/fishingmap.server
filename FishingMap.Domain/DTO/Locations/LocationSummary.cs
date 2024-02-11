using System.Collections.Generic;
using FishingMap.Domain.DTO.Geometries;
using FishingMap.Domain.DTO.Images;
using FishingMap.Domain.DTO.Species;

namespace FishingMap.Domain.DTO.Locations
{
    public class LocationSummary
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public GeoPoint Position { get; set; }
        public IEnumerable<SpeciesIdName> Species { get; set; }
        public IEnumerable<ImageDTO> Images { get; set; }
    }
}
