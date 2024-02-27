﻿using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.DTO.Users
{
    public class UserDetailsUpdate
    {
        [Required]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
