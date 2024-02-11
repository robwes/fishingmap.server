using FishingMap.Domain.DTO.Geometries;
using FishingMap.Domain.DTO.Species;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishingMap.Domain.DTO.Locations
{
    public class LocationMarker
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public GeoPoint Position { get; set; }
        public IEnumerable<SpeciesIdName> Species { get; set; }
    }
}