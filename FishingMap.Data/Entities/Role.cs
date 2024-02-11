using FishingMap.Data.Interfaces;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace FishingMap.Data.Entities
{
    public class Role : IEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }
}
