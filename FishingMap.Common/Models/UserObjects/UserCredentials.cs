﻿using System.ComponentModel.DataAnnotations;

namespace FishingMap.Common.Models.UserObjects
{
    public class UserCredentials
    {
        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MinLength(7), MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MinLength(7), MaxLength(100)]
        public string Salt { get; set; } = string.Empty;
    }
}