using System.Collections.Generic;
using FishingMap.Domain.Data.DTO.ImageObjects;

namespace FishingMap.Domain.Data.DTO.SpeciesObjects
{
    public class SpeciesDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<ImageDTO> Images { get; set; }
    }
}
