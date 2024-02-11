using FishingMap.Data.Entities;

namespace FishingMap.Data.Interfaces
{
    public interface ISpeciesRepository : IRepository<Species>
    {
        Task<IEnumerable<Species>> FindSpecies(string nameSearch = "");
        Task<Species?> GetSpeciesWithImages(int id, bool noTracking = false);
    }
}
