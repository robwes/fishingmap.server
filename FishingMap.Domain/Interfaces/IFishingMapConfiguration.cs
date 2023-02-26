using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Interfaces
{
    public interface IFishingMapConfiguration
    {
        string DatabaseConnectionString { get; }
        string ImagesFolderPath { get; }
        string FileShareName { get; }
        string FileShareConnectionString { get; }
        string LocationsImageFolderPath { get; }
        string SpeciesImageFolderPath { get; }
        string GetPathToLocationsImageFolder(int locationId);
        string GetPathToSpeciesImageFolder(int speciesId);
    }
}
