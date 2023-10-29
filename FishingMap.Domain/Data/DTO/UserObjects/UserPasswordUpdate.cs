using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Data.DTO.UserObjects
{
    public class UserPasswordUpdate
    {
        [Required]
        [MinLength(7)]
        public string CurrentPassword { get; set; }
        [Required]
        [MinLength(7)]
        public string NewPassword { get; set; }
    }
}
