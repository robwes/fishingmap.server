using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.DTO.Users
{
    public class UserLogin
    {
        [Required, StringLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 7)]
        public string Password { get; set; } = string.Empty;
    }
}
