using FishingMap.Domain.DTO.Regulations;
using FishingMap.Domain.DTO.Species;

namespace FishingMap.Domain.Interfaces
{
    public interface IRegulationsService
    {
        Task<IEnumerable<SpeciesRegulationDTO>> GetRegulations();
        Task<SpeciesRegulationDTO?> GetRegulation(int id);
        Task<SpeciesRegulationDTO> AddRegulation(SpeciesRegulationAdd regulation);
        Task<SpeciesRegulationDTO> UpdateRegulation(int id, SpeciesRegulationUpdate regulation);
        Task DeleteRegulation(int id);

        Task<IEnumerable<LocationSpeciesRuleDTO>> GetEffectiveRulesForLocation(int locationId);
        Task<IEnumerable<SpeciesRegulationScopeDTO>> GetRegulationsForSpecies(int speciesId);
    }
}
