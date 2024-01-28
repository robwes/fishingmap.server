using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.Data.DTO.UserObjects
{
    public class UserDTO
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        public IEnumerable<RoleDTO> Roles { get; set; }
    }
}
