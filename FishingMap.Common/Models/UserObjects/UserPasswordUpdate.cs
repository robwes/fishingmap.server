using System.ComponentModel.DataAnnotations;

namespace FishingMap.Common.Models.UserObjects
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
