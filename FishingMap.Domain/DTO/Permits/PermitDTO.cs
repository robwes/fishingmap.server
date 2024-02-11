using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.DTO.Permits
{
    public class PermitDTO
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Url { get; set; }
    }
}
