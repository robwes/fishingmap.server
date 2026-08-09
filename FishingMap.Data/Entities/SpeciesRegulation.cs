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

        public decimal? MinimumSizeCm { get; set; }
        public decimal? MaximumSizeCm { get; set; }
        public int? BagLimit { get; set; }

        // What BagLimit is counted against. Null means the source rule doesn't say —
        // clients must not default it to "per day".
        public BagLimitBasis? BagLimitBasis { get; set; }

        public bool IsCatchAndReleaseOnly { get; set; }

        public bool MustReportCatch { get; set; }

        [MaxLength(5000)]
        public string? AdditionalRules { get; set; }

        public virtual ICollection<ProtectedPeriod> ProtectedPeriods { get; set; } = new HashSet<ProtectedPeriod>();

        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}
