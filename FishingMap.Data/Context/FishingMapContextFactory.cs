using FishingMap.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FishingMap.Data.Context
{
    public class FishingMapContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        // UserSecretsId from FishingMap.API.csproj — where the connection string lives locally
        private const string ApiUserSecretsId = "d1c84955-d3b6-474a-a2bd-c9bf1cdbe974";

        private readonly IFishingMapConfiguration? _configuration;

        public FishingMapContextFactory() { }

        public FishingMapContextFactory(IFishingMapConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            string? connectionString;

            if (_configuration != null)
            {
                connectionString = _configuration.DatabaseConnectionString;
            }
            else
            {
                // Design-time path (dotnet ef): read from env vars or API user secrets
                var config = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddUserSecrets(ApiUserSecretsId)
                    .Build();

                connectionString = config.GetConnectionString("FishingMapDatabase");
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString, o => o.UseNetTopologySuite());
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
