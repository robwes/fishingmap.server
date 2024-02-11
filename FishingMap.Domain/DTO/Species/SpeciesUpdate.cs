using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace FishingMap.Domain.DTO.Species
{
    public class SpeciesUpdate
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<IFormFile> Images { get; set; }
    }
}
