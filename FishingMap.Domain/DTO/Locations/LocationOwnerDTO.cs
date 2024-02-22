namespace FishingMap.Domain.DTO.Locations
{
    public class LocationOwnerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? StreetAddress { get; set; }
        public string? ZipCode { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        public string? WebSite { get; set; }
        public string? Email { get; set; }
        public ICollection<LocationDTO> Locations { get; set; } = new List<LocationDTO>();
    }
}
