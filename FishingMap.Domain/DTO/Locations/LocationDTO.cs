using System;
using System.Collections.Generic;
using System.Text;
using FishingMap.Domain.DTO.Geometries;
using FishingMap.Domain.DTO.Images;
using FishingMap.Domain.DTO.Permits;
using FishingMap.Domain.DTO.Species;

namespace FishingMap.Domain.DTO.Locations
{
    public class LocationDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Rules { get; set; }
        public string Geometry { get; set; }
        public GeoPoint Position { get; set; }
        public GeoPoint NavigationPosition { get; set; }
        public string WebSite { get; set; }
        public IEnumerable<SpeciesIdName> Species { get; set; }
        public IEnumerable<PermitDTO> Permits { get; set; }
        public IEnumerable<ImageDTO> Images { get; set; }
    }
}
