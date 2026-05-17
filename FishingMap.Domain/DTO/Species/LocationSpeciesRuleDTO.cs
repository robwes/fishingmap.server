namespace FishingMap.Domain.DTO.Species
{
    public class LocationSpeciesRuleDTO
    {
        public int SpeciesId { get; set; }

        // Where the resolved rule comes from: "Location", "Region: <name>", or "National".
        public string Source { get; set; } = string.Empty;

        public decimal? MinimumSizeCm { get; set; }
        public decimal? MaximumSizeCm { get; set; }
        public int? BagLimit { get; set; }
        public bool IsCatchAndReleaseOnly { get; set; }
        public bool MustReportCatch { get; set; }

        public string? AdditionalRules { get; set; }

        public IEnumerable<ProtectedPeriodDTO> ProtectedPeriods { get; set; } = new List<ProtectedPeriodDTO>();
    }
}
