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

        // The species a water inherits region rules for. Needed alongside the resolved rules
        // because "follows a region that sets no rule" and "nobody has decided" both produce
        // no rule, and the UI must not phrase them the same way.
        Task<IEnumerable<int>> GetFollowedSpeciesIds(int locationId);

        Task SetFollowsRegion(int locationId, int speciesId, bool follows);
        Task<IEnumerable<SpeciesRegulationScopeDTO>> GetRegulationsForSpecies(int speciesId);
    }
}
