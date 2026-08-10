using FishingMap.Data.Entities;
using FishingMap.Domain.DTO.Locations;
using FishingMap.Domain.DTO.Regions;
using FishingMap.Domain.DTO.Species;

namespace FishingMap.Domain.DTO.Regulations
{
    // One regulation for a species, carrying the NAMES of whatever it is scoped to.
    // SpeciesRegulationDTO only exposes ids, which would force the species details screen to
    // fetch every region and location just to render a label.
    //
    // Scope is XOR, same as the entity: either Region is set and Locations is empty, or
    // Locations is non-empty and Region is null.
    public class SpeciesRegulationScopeDTO
    {
        public int Id { get; set; }
        public int SpeciesId { get; set; }

        public RegionDTO? Region { get; set; }
        public IEnumerable<LocationIdName> Locations { get; set; } = new List<LocationIdName>();

        // Null means the rule applies whatever the fin looks like.
        public AdiposeFin? AdiposeFin { get; set; }

        public decimal? MinimumSizeCm { get; set; }
        public decimal? MaximumSizeCm { get; set; }
        public int? BagLimit { get; set; }
        public BagLimitBasis? BagLimitBasis { get; set; }
        public bool IsCatchAndReleaseOnly { get; set; }
        public bool IsFullyProtected { get; set; }
        public bool MustReportCatch { get; set; }
        public string? AdditionalRules { get; set; }

        public IEnumerable<ProtectedPeriodDTO> ProtectedPeriods { get; set; } = new List<ProtectedPeriodDTO>();
    }
}
