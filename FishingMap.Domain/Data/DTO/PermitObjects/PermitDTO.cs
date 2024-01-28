using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.Data.DTO.PermitObjects
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
