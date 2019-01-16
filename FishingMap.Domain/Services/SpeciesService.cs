using AutoMapper;
using FishingMap.Domain.Data.Context;
using FishingMap.Domain.Data.DTO;
using FishingMap.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishingMap.Domain.Services
{
    public class SpeciesService : ISpeciesService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SpeciesService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Species> AddSpecies(Species species)
        {
            var entity = new Data.Entities.Species
            {
                Name = species.Name,
                Description = species.Description,
                Created = DateTime.Now,
                Modified = DateTime.Now
            };

            if (!await _context.Species.AnyAsync(s => s.Name == species.Name))
            {
                entity = _context.Species.Add(entity).Entity;
                await _context.SaveChangesAsync();
                return _mapper.Map<Data.Entities.Species, Species>(entity);
            }

            return null;
        }

        public async Task<IEnumerable<Species>> GetSpecies()
        {
            var species = await _context.Species.AsNoTracking().Select(s => new Species { Id = s.Id, Description = s.Description, Name = s.Name }).ToListAsync();
            return species;
        }

        public async Task<Species> UpdateSpecies(Species species)
        {
            var ent = await _context.Species.FindAsync(species.Id);
            if (ent != null)
            {
                ent.Name = species.Name;
                ent.Description = species.Description;
                ent.Modified = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return _mapper.Map<Data.Entities.Species, Species>(ent);
        }

        public async Task DeleteSpecies(int id)
        {
            var ent = await _context.Species.FirstOrDefaultAsync(s => s.Id == id);
            if (ent != null)
            {
                _context.Species.Remove(ent);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Species> GetSpeciesById(int id)
        {
            var ent = await _context.Species.FirstOrDefaultAsync(s => s.Id == id);
            if (ent == null)
            {
                return _mapper.Map<Data.Entities.Species, Species>(ent);
            }

            return null;
        }
    }
}
