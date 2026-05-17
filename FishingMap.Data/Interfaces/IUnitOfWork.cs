namespace FishingMap.Data.Interfaces
{
    public interface IUnitOfWork
    {
        IImageRepository Images { get; }
        ILocationOwnerRepository LocationOwners { get; }
        ILocationRepository Locations { get; }
        IPermitRepository Permits { get; }
        IRegionRepository Regions { get; }
        IRoleRepository Roles { get; }
        ISpeciesRepository Species { get; }
        ISpeciesRegulationRepository SpeciesRegulations { get; }
        IUserRepository Users { get; }
        Task SaveChanges();
    }
}
