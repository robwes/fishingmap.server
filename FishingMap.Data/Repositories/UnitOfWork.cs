using FishingMap.Data.Context;
using FishingMap.Data.Interfaces;

namespace FishingMap.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IImageRepository Images { get; private set; }
        public ILocationOwnerRepository LocationOwners { get; private set; }
        public ILocationRepository Locations { get; private set; }
        public IPermitRepository Permits { get; private set; }
        public IRoleRepository Roles { get; private set; }
        public ISpeciesRepository Species { get; private set; }
        public IUserRepository Users { get; private set; }

        public UnitOfWork(ApplicationDbContext context,
                          IImageRepository imageRepository,
                          ILocationOwnerRepository locationOwnerRepository,
                          ILocationRepository locationRepository,
                          IPermitRepository permitRepository,
                          IRoleRepository roleRepository,
                          ISpeciesRepository speciesRepository,
                          IUserRepository userRepository)
        {
            _context = context;
            Images = imageRepository;
            LocationOwners = locationOwnerRepository;
            Locations = locationRepository;
            Permits = permitRepository;
            Roles = roleRepository;
            Species = speciesRepository;
            Users = userRepository;
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
