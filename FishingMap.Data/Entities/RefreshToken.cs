using FishingMap.Data.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Data.Entities
{
    public class RefreshToken : IEntity
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // SHA-256 of the raw token — the raw value is only ever held by the client
        [Required]
        [MaxLength(64)]
        public string TokenHash { get; set; } = string.Empty;

        [Required]
        public DateTime Expires { get; set; }

        [Required]
        public DateTime Created { get; set; }

        // Set when rotated, explicitly revoked, or invalidated by reuse detection
        public DateTime? Revoked { get; set; }
    }
}
