using System;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.Data.Entities
{
    public class Image
    {
        public Image()
        { }

        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
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
