using System.Collections.Generic;
using FishingMap.Domain.Data.DTO.GeoObjects;
using FishingMap.Domain.Data.DTO.ImageObjects;
using FishingMap.Domain.Data.DTO.SpeciesObjects;

namespace FishingMap.Domain.Data.DTO.LocationObjects
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
