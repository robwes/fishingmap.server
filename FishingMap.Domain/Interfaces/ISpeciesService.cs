using FishingMap.Domain.Data.DTO.SpeciesObjects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Interfaces
{
    public interface ISpeciesService
    {
        Task<SpeciesDTO> AddSpecies(SpeciesAdd species);
        Task<IEnumerable<SpeciesDTO>> GetSpecies(string search = "");
        Task<SpeciesDTO> GetSpeciesById(int id);
        Task<SpeciesDTO> UpdateSpecies(int id, SpeciesUpdate species);
        Task DeleteSpecies(int id);
    }
}
