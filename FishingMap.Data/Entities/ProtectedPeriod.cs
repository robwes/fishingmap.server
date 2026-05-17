using FishingMap.Data.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FishingMap.Data.Entities
{
    // Stores each individual protected period 
    public class ProtectedPeriod : IEntity
    {
        public int Id { get; set; }
        
        // Foreign key back to the regulation
        public int SpeciesRegulationId { get; set; }
        public virtual SpeciesRegulation Regulation { get; set; } = null!;

        [Range(1, 12)]
        public int StartMonth { get; set; }
        
        [Range(1, 31)]
        public int StartDay { get; set; }
        
        [Range(1, 12)]
        public int EndMonth { get; set; }
        
        [Range(1, 31)]
        public int EndDay { get; set; }

        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
    }
}
