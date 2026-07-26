using FishingMap.Domain.DTO.Common;

namespace FishingMap.Domain.DTO.Species
{
    public class SpeciesInfoPatch
    {
        public Optional<string> Name { get; set; }
        public Optional<string?> ScientificName { get; set; }
        public Optional<string?> Description { get; set; }
    }
}
