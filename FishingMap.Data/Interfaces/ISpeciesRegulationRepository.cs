using FishingMap.Data.Entities;

namespace FishingMap.Data.Interfaces
{
    public interface ISpeciesRegulationRepository : IRepository<SpeciesRegulation>
    {
        Task<IReadOnlyList<SpeciesRegulation>> GetCandidatesForLocation(int locationId, IEnumerable<int> ancestorRegionIds);

        // The REGION-scoped rules for one species, with Region loaded so callers can render
        // names rather than ids. Deliberately not every rule: the species page is an
        // angler's view of what the law says generally, and the location-scoped rules are
        // one row per water that diverges — unbounded, and a maintainer's question rather
        // than a reader's. See robwes/fishingmap.web#13.
        Task<IReadOnlyList<SpeciesRegulation>> GetRegionRulesForSpecies(int speciesId);
    }
}
