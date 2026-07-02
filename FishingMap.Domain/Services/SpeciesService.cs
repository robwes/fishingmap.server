using MapsterMapper;
using FishingMap.Common.Extensions;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.DTO.Species;
using FishingMap.Domain.Interfaces;
using FishingMap.Domain.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FishingMap.Domain.Services
{
    public class SpeciesService : ISpeciesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IFishingMapConfiguration _config;
        private readonly IMapper _mapper;

        public SpeciesService(IUnitOfWork unitOfWork, IFileService fileService, IFishingMapConfiguration config, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _config = config;
            _mapper = mapper;
        }

        public async Task<SpeciesDTO> AddSpecies(SpeciesAdd species)
        {
            var entity = new Species
            {
                Name = species.Name,
                ScientificName = species.ScientificName,
                Description = species.Description,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            };

            if (await _unitOfWork.Species.Any(s => s.Name == species.Name))
            {
                throw new ArgumentException($"A species with the name {species.Name} already exists.");
            }

            entity = _unitOfWork.Species.Add(entity);
            await _unitOfWork.SaveChanges();

            if (species.Images?.Count > 0)
            {
                entity.Images = new List<Image>();
                foreach (var image in species.Images)
                {
                    await AddSpeciesImage(entity, image);
                }
                await _unitOfWork.SaveChanges();
            }

            return _mapper.Map<SpeciesDTO>(entity);
        }

        public async Task DeleteSpecies(int id)
        {
            var species = await _unitOfWork.Species.GetSpeciesWithImages(id);
            if (species != null)
            {
                foreach (var image in species.Images)
                {
                    _unitOfWork.Images.Delete(image);
                }

                _unitOfWork.Species.Delete(species);
                await _unitOfWork.SaveChanges();

                await _fileService.DeleteFolder(_config.GetPathToSpeciesImageFolder(species.Id));
            }
        }

        public async Task<IEnumerable<SpeciesDTO>> GetSpecies(string search = "")
        {
            var species = await _unitOfWork.Species.FindSpecies(search);
            return _mapper.Map<IEnumerable<SpeciesDTO>>(species);
        }

        public async Task<SpeciesDTO?> GetSpeciesById(int id)
        {
            var species = await _unitOfWork.Species.GetSpeciesWithImages(id, true);
            if (species != null)
            {
                return _mapper.Map<SpeciesDTO>(species);
            }

            return null;
        }

        public async Task<SpeciesDTO> UpdateSpecies(int id, SpeciesUpdate species)
        {
            var entity = await _unitOfWork.Species.GetSpeciesWithImages(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Species with id {id} not found.");
            }

            if (entity.Name != species.Name &&
                await _unitOfWork.Species.Any(s => s.Name == species.Name && s.Id != id))
            {
                throw new ArgumentException($"A species with the name {species.Name} already exists.");
            }

            entity.Name = species.Name;
            entity.ScientificName = species.ScientificName;
            entity.Description = species.Description;

            await UpdateSpeciesImages(entity, species);
            entity.Modified = DateTime.UtcNow;

            await _unitOfWork.SaveChanges();

            return _mapper.Map<SpeciesDTO>(entity);
        }

        private async Task AddSpeciesImage(Species species, IFormFile image)
        {
            ImageUpload.Validate(image);

            var filePath = await _fileService.AddFile(
                image,
                $"species/{species.Id}"
            );
            var fileName = Path.GetFileName(filePath);

            if (species.Images == null)
            {
                species.Images = new List<Image>();
            }

            species.Images.Add(new Image
            {
                Name = fileName,
                Path = filePath,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            });
        }

        private async Task DeleteSpeciesImage(Species species, Image image)
        {
            species.Images.Remove(image);
            _unitOfWork.Images.Delete(image);
            await _fileService.DeleteFile(image.Path);
        }

        private async Task UpdateSpeciesImages(Species speciesEntity, SpeciesUpdate speciesUpdate)
        {
            if (speciesUpdate.Images == null)
            {
                return;
            }

            if (!speciesEntity.Images.IsNullOrEmpty())
            {
                var imagesInUpdateModel = speciesUpdate.Images.Select(img => img.FileName);
                var imagesToDelete = speciesEntity.Images.Where(img => !imagesInUpdateModel.Contains(img.Name)).ToList();

                foreach (var image in imagesToDelete)
                {
                    await DeleteSpeciesImage(speciesEntity, image);
                }
            }

            var imagesInEntityModel = speciesEntity.Images?.Select(img => img.Name) ?? new List<string>();
            var imagesToAdd = speciesUpdate.Images!.Where(i => !imagesInEntityModel.Contains(i.FileName)).ToList();

            foreach (var image in imagesToAdd)
            {
                await AddSpeciesImage(speciesEntity, image);
            }
        }
    }
}
