using FishingMap.Data.Entities;

namespace FishingMap.Domain.DTO.Species
{
    public class LocationSpeciesRuleDTO
    {
        public int SpeciesId { get; set; }

        // The underlying SpeciesRegulation this resolved to. The location editor needs it to
        // save an edit against the right record instead of guessing.
        public int RegulationId { get; set; }

        // Every location the underlying regulation applies to. More than one (besides the
        // location being queried) means editing it here would change other waters too, so
        // the editor renders the rule read-only.
        public IEnumerable<int> LocationIds { get; set; } = new List<int>();

        // Where the resolved rule comes from: "Location", "Region: <name>", or "National".
        public string Source { get; set; } = string.Empty;

        // Which fish this resolved rule covers. Null means all of them; otherwise a species
        // yields one LocationSpeciesRuleDTO per fin state that has a rule, so the client
        // renders a species as several labelled blocks rather than one row.
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

        // The rule that would apply if this one were removed — the next most specific
        // candidate for the same species. Null when nothing else covers this water.
        //
        // Without this the editor cannot tell a maintainer what "revert to the inherited
        // rule" would fall back to, because the resolver returns only the winner. Nested one
        // level deep only: this property is always null on the nested instance.
        public LocationSpeciesRuleDTO? FallsBackTo { get; set; }
    }
}
