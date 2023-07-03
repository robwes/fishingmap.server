using System;
using System.Collections.Generic;
using System.Text;

namespace FishingMap.Domain.Data.DTO
{
    public class LocationMarker
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public GeoPoint Position { get; set; }
        public IEnumerable<string> Species { get; set; }
    }
}
