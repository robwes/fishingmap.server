using FishingMap.Domain.DTO.Species;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
