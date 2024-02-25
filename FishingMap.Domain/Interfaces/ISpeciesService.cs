using FishingMap.Domain.DTO.Species;

namespace FishingMap.Domain.Interfaces
{
    public interface ISpeciesService
    {
        Task<SpeciesDTO> AddSpecies(SpeciesAdd species);
        Task DeleteSpecies(int id);
        Task<IEnumerable<SpeciesDTO>> GetSpecies(string search = "");
        Task<SpeciesDTO> GetSpeciesById(int id);
        Task<SpeciesDTO> UpdateSpecies(int id, SpeciesUpdate species);
    }
}
