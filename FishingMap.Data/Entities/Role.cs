using FishingMap.Data.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Data.Entities
{
    public class Role : IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<User> Users { get; set; } = new HashSet<User>();
    }
}
