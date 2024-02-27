using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.DTO.Permits
{
    public class PermitAdd
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Url { get; set; }
    }
}
