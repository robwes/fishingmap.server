using FishingMap.Domain.Data.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Interfaces
{
    public interface ISpeciesService
    {
        Task<Species> AddSpecies(SpeciesUpdate species);
        Task<IEnumerable<Species>> GetSpecies(string search = "");
        Task<Species> GetSpeciesById(int id);
        Task<Species> UpdateSpecies(int id, SpeciesUpdate species);
        Task DeleteSpecies(int id);
    }
}
