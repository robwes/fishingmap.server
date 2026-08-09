using FishingMap.Data.Entities;

namespace FishingMap.Data.Interfaces
{
    public interface ISpeciesRegulationRepository : IRepository<SpeciesRegulation>
    {
        Task<IReadOnlyList<SpeciesRegulation>> GetCandidatesForLocation(int locationId, IEnumerable<int> ancestorRegionIds);

        // Every regulation for one species, with Region and Locations loaded so callers can
        // render names rather than ids.
        Task<IReadOnlyList<SpeciesRegulation>> GetForSpecies(int speciesId);
    }
}
