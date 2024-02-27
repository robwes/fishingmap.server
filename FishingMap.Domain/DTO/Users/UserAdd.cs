using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.DTO.Users
{
    public class UserAdd
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 7)]
        public string Password { get; set; } = string.Empty;
    }
}
