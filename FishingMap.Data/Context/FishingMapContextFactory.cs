using FishingMap.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FishingMap.Data.Context
{
    public class FishingMapContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        private readonly IFishingMapConfiguration? _configuration;

        public FishingMapContextFactory()
        {
            
        }

        public FishingMapContextFactory(IFishingMapConfiguration configuration)
        {
            _configuration= configuration;
        }

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = _configuration?.DatabaseConnectionString;
            optionsBuilder.UseSqlServer(connectionString, o => o.UseNetTopologySuite());
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
