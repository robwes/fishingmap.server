using FishingMap.Domain.Data.DTO.GeoObjects;
using FishingMap.Domain.Data.DTO.SpeciesObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishingMap.Domain.Data.DTO.LocationObjects
{
    public class LocationMarker
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public GeoPoint Position { get; set; }
        public IEnumerable<SpeciesIdName> Species { get; set; }
    }
}