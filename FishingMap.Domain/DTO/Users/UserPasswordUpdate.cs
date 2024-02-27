using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.DTO.Users
{
    public class UserPasswordUpdate
    {
        [Required, StringLength(100, MinimumLength = 7)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 7)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
