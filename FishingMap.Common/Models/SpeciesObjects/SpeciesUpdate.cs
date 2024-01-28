using Microsoft.AspNetCore.Http;

namespace FishingMap.Common.Models.SpeciesObjects
{
    public class SpeciesUpdate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
