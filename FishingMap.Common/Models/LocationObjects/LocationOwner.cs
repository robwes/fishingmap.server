namespace FishingMap.Common.Models.LocationObjects
{
    public class LocationOwner
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string StreetAddress { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string WebSite { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;   
        public ICollection<Location> Locations { get; set; } = new List<Location>();
    }
}
