namespace FishingMap.Common.Interfaces
{
    public interface IFishingMapConfiguration
    {
        string DatabaseConnectionString { get; }
        string FileShareName { get; }
        string FileShareConnectionString { get; }
        string LocationsImageFolderPath { get; }
        string SpeciesImageFolderPath { get; }
        string GetPathToLocationsImageFolder(int locationId);
        string GetPathToSpeciesImageFolder(int speciesId);
    }
}
