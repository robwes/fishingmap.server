using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FishingMap.Domain.Data.Entities
{
    public class Species
    {
        public Species()
        {}

        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public DateTime Created { get; set; }
        [Required]
        public DateTime Modified { get; set; }
        public virtual ICollection<Location> Locations { get; set; }
        public virtual ICollection<Image> Images { get; set; }
    }
}
