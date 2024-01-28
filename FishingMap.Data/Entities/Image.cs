using FishingMap.Data.Interfaces;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace FishingMap.Data.Entities
{
    public class Image : IEntity
    {
        public Image()
        { }

        [Required]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = default!;

        [Required]
        public string Path { get; set; }

        public string Author { get; set; }
        public string AuthorLink { get; set; }

        [Required]
        public DateTime Created { get; set; }
        [Required]
        public DateTime Modified { get; set; }
    }
}
