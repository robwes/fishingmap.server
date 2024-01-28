﻿using System.ComponentModel.DataAnnotations;

namespace FishingMap.Common.Models.UserObjects
{
    public class UserAdd
    {
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
    }
}