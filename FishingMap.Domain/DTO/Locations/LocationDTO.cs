using FishingMap.Domain.DTO.Geometries;
using FishingMap.Domain.DTO.Images;
using FishingMap.Domain.DTO.Permits;
using FishingMap.Domain.DTO.Regions;
using FishingMap.Domain.DTO.Species;

namespace FishingMap.Domain.DTO.Locations
{
    public class LocationDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Rules { get; set; }
        public string Geometry { get; set; } = string.Empty;

        public GeoPoint Position { get; set; } = new GeoPoint();

        public GeoPoint? NavigationPosition { get; set; }
        public string? WebSite { get; set; }

        public RegionDTO? Region { get; set; }

        public IEnumerable<SpeciesIdName> Species { get; set; } = new List<SpeciesIdName>();
        public IEnumerable<ImageDTO> Images { get; set; } = new List<ImageDTO>();
        public IEnumerable<PermitDTO> Permits { get; set; } = new List<PermitDTO>();
        public IEnumerable<LocationSpeciesRuleDTO> SpeciesRules { get; set; } = new List<LocationSpeciesRuleDTO>();

        // Species this water inherits its region's rules for. Read it together with
        // SpeciesRules: a Location-sourced rule means a custom rule, an id here means
        // inherited, and neither means no administrator has decided yet — which is not the
        // same as "unrestricted" and must not be shown as such.
        public IEnumerable<int> FollowsRegionSpeciesIds { get; set; } = new List<int>();
    }
}
