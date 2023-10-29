using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace FishingMap.Domain.Data.DTO.SpeciesObjects
{
    public class SpeciesUpdate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<IFormFile> Images { get; set; }
    }
}
