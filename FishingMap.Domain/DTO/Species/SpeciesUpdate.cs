using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.DTO.Species
{
    public class SpeciesUpdate
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
