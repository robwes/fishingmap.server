using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.Data.DTO
{
    public class Permit
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Url { get; set; }
    }
}
