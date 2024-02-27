using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.DTO.Images
{
    public class ImageDTO
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Path { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Author { get; set; }

        [StringLength(255)]
        public string? AuthorLink { get; set; }
    }
}
