using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.DTO.Users
{
    public class UserPasswordUpdate
    {
        [Required]
        [MinLength(7)]
        public string CurrentPassword { get; set; } = string.Empty;
        [Required]
        [MinLength(7)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
