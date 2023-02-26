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
        public string DatabaseConnectionString => _configuration["ConnectionStrings:FishingMapDatabase"];

        public string ImagesFolderPath => _configuration["AppSeettings:ImagesFolderPath"];

        public string FileShareName => _configuration["FileShare:Name"];
        public string FileShareConnectionString => _configuration.GetConnectionString("FishingMapStorage");
        public string LocationsImageFolderPath => _configuration["FileShare:LocationsImageFolderPath"];
        public string SpeciesImageFolderPath => _configuration["FileShare:SpeciesImageFolderPath"];
        public string GetPathToLocationsImageFolder(int locationId) => $"{LocationsImageFolderPath}/{locationId}";
        public string GetPathToSpeciesImageFolder(int speciesId) => $"{SpeciesImageFolderPath}/{speciesId}";
    }
}
