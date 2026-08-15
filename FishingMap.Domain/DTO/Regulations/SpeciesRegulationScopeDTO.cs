using FishingMap.Data.Entities;
using FishingMap.Domain.DTO.Regions;
using FishingMap.Domain.DTO.Species;

namespace FishingMap.Domain.DTO.Regulations
{
    // One REGION-scoped regulation for a species, carrying the region's NAME.
    // SpeciesRegulationDTO only exposes ids, which would force the species details screen to
    // fetch every region just to render a label.
    //
    // Location-scoped rules are deliberately absent. The species page answers an angler's
    // question — what does the law say about this fish — and the per-water exceptions are
    // one row per water that diverges: unbounded as the site grows, and a maintainer's view
    // rather than a reader's. They get their own admin screen when one exists. Because of
    // that, Region is always set here. See robwes/fishingmap.web#13.
    public class SpeciesRegulationScopeDTO
    {
        public int Id { get; set; }
        public int SpeciesId { get; set; }

        public RegionDTO? Region { get; set; }

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
