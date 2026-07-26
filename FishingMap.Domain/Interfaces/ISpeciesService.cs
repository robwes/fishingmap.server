using FishingMap.Domain.DTO.Images;
using FishingMap.Domain.DTO.Species;
using Microsoft.AspNetCore.Http;

namespace FishingMap.Domain.Interfaces
{
    public interface ISpeciesService
    {
        Task<SpeciesDTO> AddSpecies(SpeciesAdd species);
        Task DeleteSpecies(int id);
        Task<IEnumerable<SpeciesDTO>> GetSpecies(string search = "");
        Task<SpeciesDTO?> GetSpeciesById(int id);
        Task<SpeciesDTO> UpdateSpecies(int id, SpeciesUpdate species);
        Task<SpeciesDTO> UpdateSpeciesInfo(int id, SpeciesInfoPatch patch);
        Task<ImageDTO> AddImageToSpecies(int id, IFormFile image);
        Task RemoveImageFromSpecies(int id, int imageId);
    }
}
