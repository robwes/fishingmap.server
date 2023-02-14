using FishingMap.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FishingMap.API
{
    public class FishingMapConfiguration : IFishingMapConfiguration
    {
        private readonly IConfiguration _configuration;
        public FishingMapConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string DatabaseConnectionString => _configuration["ConnectionStrings:UusimapDatabase"];

        public string ImagesFolderPath => _configuration["AppSeettings:ImagesFolderPath"];
    }
}
