using FishingMap.Domain.Data.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Interfaces
{
    public interface ISpeciesService
    {
        Task<Species> AddSpecies(Species species);
        Task<IEnumerable<Species>> GetSpecies();
        Task<Species> GetSpeciesById(int id);
        Task<Species> UpdateSpecies(Species species);
        Task DeleteSpecies(int id);
    }
}
