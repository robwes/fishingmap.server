using FishingMap.Data.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Data.Entities
{
    public class User : IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(7), MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MinLength(7), MaxLength(100)]
        public string Salt { get; set; } = string.Empty;

        [Required]
        public DateTime Created { get; set; }
        [Required]
        public DateTime Modified { get; set; }

        public ICollection<Role> Roles { get; set; } = new HashSet<Role>();
    }
}
