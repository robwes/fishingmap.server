using FishingMap.Data.Entities;

namespace FishingMap.Data.Interfaces
{
    public interface IPermitRepository : IRepository<Permit>
    {
        Task<IEnumerable<Permit>> FindPermits(string nameSearch = "");
    }
}
