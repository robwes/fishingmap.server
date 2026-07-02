using FishingMap.Domain.DTO.Common;

namespace FishingMap.Domain.DTO.Locations
{
    public class LocationInfoPatch
    {
        public Optional<string> Name { get; set; }
        public Optional<string?> Description { get; set; }
        public Optional<string?> Rules { get; set; }
        public Optional<string?> WebSite { get; set; }
        public Optional<int?> RegionId { get; set; }
    }
}
