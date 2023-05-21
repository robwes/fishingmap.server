using FishingMap.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace FishingMap.Domain.Data.Context
{
    public class FishingMapContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        private readonly IFishingMapConfiguration _configuration;

        public FishingMapContextFactory() {}

        public FishingMapContextFactory(IFishingMapConfiguration configuration)
        {
            _configuration= configuration;
        }

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = _configuration.DatabaseConnectionString;
            optionsBuilder.UseSqlServer(connectionString, o => o.UseNetTopologySuite());
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
