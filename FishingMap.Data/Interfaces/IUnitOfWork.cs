namespace FishingMap.Data.Interfaces
{
    public interface IUnitOfWork
    {
        IImageRepository Images { get; }
        ILocationOwnerRepository LocationOwners { get; }
        ILocationRepository Locations { get; }
        IPermitRepository Permits { get; }
        IRoleRepository Roles { get; }
        ISpeciesRepository Species { get; }   
        IUserRepository Users { get; }
        Task SaveChanges();    
    }
}
