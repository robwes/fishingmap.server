using FishingMap.Data.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Data.Entities
{
    public class SpeciesRegulation : IEntity
    {
        public int Id { get; set; }

        public int SpeciesId { get; set; }
        public virtual Species Species { get; set; } = null!;

        // Region-scoped rule (national rules point to the Region row with Type = National).
        // Mutually exclusive with Locations: exactly one of (RegionId, Locations non-empty) must be set.
        public int? RegionId { get; set; }
        public virtual Region? Region { get; set; }

        // Location-scoped rule. M:M so a single regulation can apply to multiple specific waters.
        public virtual ICollection<Location> Locations { get; set; } = new HashSet<Location>();

        // Narrows the rule to fish with a particular adipose fin state. Null means it applies
        // whatever the fin looks like, which is every rule written before this field existed.
        //
        // This is a filter on the rule, NOT a specificity tier: resolution keeps the region
        // cascade as its primary axis and only prefers an exact fin match over a null one
        // between candidates that already tie on region.
        public AdiposeFin? AdiposeFin { get; set; }

        public decimal? MinimumSizeCm { get; set; }
        public decimal? MaximumSizeCm { get; set; }
        public int? BagLimit { get; set; }

        // What BagLimit is counted against. Null means the source rule doesn't say —
        // clients must not default it to "per day".
        public BagLimitBasis? BagLimitBasis { get; set; }

        public bool IsCatchAndReleaseOnly { get; set; }

        // The species may not be taken at all here. Distinct from IsCatchAndReleaseOnly:
        // catch-and-release is a permitted way to fish, while full protection means the fish
        // is off-limits and an accidental catch must go back. Conflating the two would have
        // the app tell anglers it is fine to target a protected fish. See #16.
        public bool IsFullyProtected { get; set; }

        public bool MustReportCatch { get; set; }

        [MaxLength(5000)]
        public string? AdditionalRules { get; set; }

        public virtual ICollection<ProtectedPeriod> ProtectedPeriods { get; set; } = new HashSet<ProtectedPeriod>();

        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}
