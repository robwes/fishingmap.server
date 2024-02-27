using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.DTO.Species
{
    public class SpeciesAdd
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(5000)]
        public string? Description { get; set; }
        public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    }
}
