using System;
using System.Collections.Generic;
using System.Text;

namespace FishingMap.Domain.Data.DTO
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string LicenseInfo { get; set; }
        public string Rules { get; set; }
        public string Geometry { get; set; }
        public GeoPoint Position { get; set; }
        public string WebSite { get; set; }
        public IEnumerable<Species> Species { get; set; }
        public IEnumerable<Image> Images { get; set; }
    }
}
