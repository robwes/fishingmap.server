using AutoMapper;
using FishingMap.Domain.Data.Context;
using FishingMap.Domain.Data.DTO;
using FishingMap.Domain.Extensions;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FishingMap.Domain.Services
{
    public class SpeciesService : ISpeciesService
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly IFishingMapConfiguration _config;
        private readonly IMapper _mapper;

        public SpeciesService(ApplicationDbContext context, IFileService fileService, IFishingMapConfiguration config,IMapper mapper)
        {
            _context = context;
            _fileService = fileService;
            _config = config;
            _mapper = mapper;
        }

        public async Task<Species> AddSpecies(SpeciesUpdate species)
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

                if (species.Images?.Count > 0)
                {
                    entity.Images = new List<Data.Entities.Image>();
                    foreach (var image in species.Images)
                    {
                        await AddSpeciesImage(entity, image);
                    }
                    await _context.SaveChangesAsync();
                }

                return _mapper.Map<Data.Entities.Species, Species>(entity);
            }

            return null;
        }

        public async Task<IEnumerable<Species>> GetSpecies(string search = "")
        {
            var query = _context.Species.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.Name.Contains(search));
            }

            var species = await query
                .Include(s => s.Images)               
                .OrderBy(s => s.Name)
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<Data.Entities.Species>, IEnumerable<Data.DTO.Species>>(species);
        }

        public async Task<Species> UpdateSpecies(int id, SpeciesUpdate species)
        {
            var entity = await _context.Species.Include(s => s.Images).FirstOrDefaultAsync(s => s.Id == id);
            if (entity != null)
            {
                entity.Name = species.Name;
                entity.Description = species.Description;
                
                if (entity.Images == null)
                {
                    entity.Images = new List<Data.Entities.Image>();
                }

                if (species.Images?.Count > 0 && entity.Images.Count > 0)
                {
                    var imagesInUpdateModel = species.Images.Select(img => img.FileName);
                    var imagesInDb = entity.Images.Select(img => img.Name);

                    var imagesToDelete = entity.Images.Where(img => !imagesInUpdateModel.Contains(img.Name));
                    foreach (var image in imagesToDelete)
                    {
                        await DeleteSpeciesImage(entity, image);
                    }

                    var imagesToAdd = species.Images.Where(i => !imagesInDb.Contains(i.FileName));
                    foreach (var image in imagesToAdd)
                    {
                        await AddSpeciesImage(entity, image);
                    }
                }
                else if (species.Images?.Count > 0 && entity.Images.Count == 0)
                {
                    foreach (var image in species.Images)
                    {
                        await AddSpeciesImage(entity, image);
                    }
                }
                else if (species.Images.IsNullOrEmpty())
                {
                    foreach (var image in entity.Images)
                    {
                        await DeleteSpeciesImage(entity, image);
                    }
                }

                entity.Modified = DateTime.Now;
                await _context.SaveChangesAsync();

                return _mapper.Map<Data.Entities.Species, Species>(entity);
            }

            return null;
        }

        private async Task DeleteSpeciesImage(Data.Entities.Species species, Data.Entities.Image image)
        {
            species.Images.Remove(image);
            _context.Images.Remove(image);
            await _fileService.DeleteFile(image.Path);
        }

        private async Task AddSpeciesImage(Data.Entities.Species species, IFormFile image)
        {
            var filePath = await _fileService.AddFile(
                image,
                $"species/{species.Id}"
            );
            var fileName = Path.GetFileName(filePath);

            species.Images.Add(new Data.Entities.Image
            {
                Name = fileName,
                Path = filePath,
                Created = DateTime.Now,
                Modified = DateTime.Now
            });
        }

        public async Task DeleteSpecies(int id)
        {
            var species = await _context.Species.Include(s => s.Images).FirstOrDefaultAsync(s => s.Id == id);
            if (species != null)
            {
                foreach (var image in species.Images)
                {
                    _context.Images.Remove(image);
                }

                _context.Species.Remove(species);
                await _context.SaveChangesAsync();

                await _fileService.DeleteFolder(_config.GetPathToSpeciesImageFolder(species.Id));
            }
        }

        public async Task<Species> GetSpeciesById(int id)
        {
            var ent = await _context.Species.Include(s => s.Images).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (ent != null)
            {
                return _mapper.Map<Data.Entities.Species, Species>(ent);
            }

            return null;
        }
    }
}
