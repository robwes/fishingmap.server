using FishingMap.Domain.DTO.Permits;
using FishingMap.Domain.DTO.Species;

namespace FishingMap.Domain.DTO.Locations
{
    public class LocationAssociationsPatch
    {
        public IEnumerable<SpeciesDTO> Species { get; set; } = new List<SpeciesDTO>();
        public IEnumerable<PermitDTO> Permits { get; set; } = new List<PermitDTO>();
    }
}
