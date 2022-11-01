using System;
using System.Collections.Generic;
using System.Text;

namespace FishingMap.Domain.Data.DTO
{
    public class LocationOwner
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string StreetAddress { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string WebSite { get; set; }
        public string Email { get; set; }
        public ICollection<Location> Locations { get; set; }
    }
}
