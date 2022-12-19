using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Data.DTO
{
    public class UserCredentials
    {
        [Required]
        [MaxLength(50)]
        public string UserName { get; set; }

        [Required]
        [MinLength(7), MaxLength(100)]
        public string Password { get; set; }

        [Required]
        [MinLength(7), MaxLength(100)]
        public string Salt { get; set; }
    }
}
