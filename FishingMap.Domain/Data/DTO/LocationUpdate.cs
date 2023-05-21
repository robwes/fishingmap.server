using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Data.DTO
{
    public class LocationUpdate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<IFormFile> Images { get; set; }
        public string LicenseInfo { get; set; }
        public string Rules { get; set; }
        public string Geometry { get; set; }
        public GeoPoint Position { get; set; }
        public string WebSite { get; set; }

        [FromForm]
        public IEnumerable<Species> Species { get; set; }

        [FromForm]
        public IEnumerable<Permit> Permits { get; set; }
    }
}
