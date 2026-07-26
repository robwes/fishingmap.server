using MapsterMapper;
using FishingMap.Common.Extensions;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.DTO.Images;
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

        public async Task<ImageDTO> AddImageToSpecies(int id, IFormFile image)
        {
            var entity = await _unitOfWork.Species.GetSpeciesWithImages(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Species with id {id} not found.");
            }

            var newImage = await AddSpeciesImage(entity, image);
            await _unitOfWork.SaveChanges();
            return _mapper.Map<Image, ImageDTO>(newImage);
        }

        public async Task RemoveImageFromSpecies(int id, int imageId)
        {
            var entity = await _unitOfWork.Species.GetSpeciesWithImages(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Species with id {id} not found.");
            }

            var image = entity.Images?.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                throw new KeyNotFoundException($"Image with id {imageId} not found on species {id}.");
            }

            await DeleteSpeciesImage(entity, image);
            await _unitOfWork.SaveChanges();
        }

        public async Task<SpeciesDTO> UpdateSpeciesInfo(int id, SpeciesInfoPatch patch)
        {
            var entity = await _unitOfWork.Species.GetSpeciesWithImages(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Species with id {id} not found.");
            }

            if (patch.Name.HasValue)
            {
                if (string.IsNullOrEmpty(patch.Name.Value))
                {
                    throw new ArgumentException("Name cannot be empty.");
                }

                if (entity.Name != patch.Name.Value &&
                    await _unitOfWork.Species.Any(s => s.Name == patch.Name.Value && s.Id != id))
                {
                    throw new ArgumentException($"A species with the name {patch.Name.Value} already exists.");
                }

                entity.Name = patch.Name.Value;
            }

            if (patch.ScientificName.HasValue)
            {
                entity.ScientificName = string.IsNullOrEmpty(patch.ScientificName.Value) ? null : patch.ScientificName.Value;
            }

            if (patch.Description.HasValue)
            {
                entity.Description = string.IsNullOrEmpty(patch.Description.Value) ? null : patch.Description.Value;
            }

            entity.Modified = DateTime.UtcNow;
            await _unitOfWork.SaveChanges();

            return _mapper.Map<SpeciesDTO>(entity);
        }

        private async Task<Image> AddSpeciesImage(Species species, IFormFile image)
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

            var newImage = new Image
            {
                Name = fileName,
                Path = filePath,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow
            };

            species.Images.Add(newImage);
            return newImage;
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
