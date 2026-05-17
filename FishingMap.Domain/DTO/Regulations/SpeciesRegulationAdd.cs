using FishingMap.Domain.DTO.Species;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Domain.DTO.Regulations
{
    public class SpeciesRegulationAdd
    {
        [Required]
        public int SpeciesId { get; set; }

        public int? RegionId { get; set; }
        public IEnumerable<int> LocationIds { get; set; } = new List<int>();

        public decimal? MinimumSizeCm { get; set; }
        public decimal? MaximumSizeCm { get; set; }
        public int? BagLimit { get; set; }
        public bool IsCatchAndReleaseOnly { get; set; }
        public bool MustReportCatch { get; set; }

        [StringLength(5000)]
        public string? AdditionalRules { get; set; }

        public IEnumerable<ProtectedPeriodDTO> ProtectedPeriods { get; set; } = new List<ProtectedPeriodDTO>();
    }
}
