using FishingMap.Domain.DTO.Species;

namespace FishingMap.Domain.DTO.Regulations
{
    public class SpeciesRegulationDTO
    {
        public int Id { get; set; }
        public int SpeciesId { get; set; }

        public int? RegionId { get; set; }
        public IEnumerable<int> LocationIds { get; set; } = new List<int>();

        public decimal? MinimumSizeCm { get; set; }
        public decimal? MaximumSizeCm { get; set; }
        public int? BagLimit { get; set; }
        public bool IsCatchAndReleaseOnly { get; set; }
        public bool MustReportCatch { get; set; }
        public string? AdditionalRules { get; set; }

        public IEnumerable<ProtectedPeriodDTO> ProtectedPeriods { get; set; } = new List<ProtectedPeriodDTO>();
    }
}
