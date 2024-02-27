using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.DTO.Permits
{
    public class PermitUpdate
    {
        [Required]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Url { get; set; }
    }
}
