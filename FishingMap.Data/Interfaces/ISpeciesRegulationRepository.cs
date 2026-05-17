using FishingMap.Data.Entities;

namespace FishingMap.Data.Interfaces
{
    public interface ISpeciesRegulationRepository : IRepository<SpeciesRegulation>
    {
        Task<IReadOnlyList<SpeciesRegulation>> GetCandidatesForLocation(int locationId, IEnumerable<int> ancestorRegionIds);
    }
}
