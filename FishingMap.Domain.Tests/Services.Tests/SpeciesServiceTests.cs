using AutoMapper;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using FishingMap.Domain.DTO.Species;
using Moq;
using FishingMap.Domain.Services;
using FishingMap.Domain.DTO.Images;
using Microsoft.AspNetCore.Http.Internal;
using System.Text;
using FishingMap.Domain.AutoMapperProfiles;

namespace FishingMap.Domain.Tests.Services.Tests
{
    public class SpeciesServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IFishingMapConfiguration> _configMock;
        private readonly IMapper _mapper;
        private readonly SpeciesService _speciesService;

        public SpeciesServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _fileServiceMock = new Mock<IFileService>();
            _configMock = new Mock<IFishingMapConfiguration>();

            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DomainProfile>();
            }).CreateMapper();

            var speciesMock = new Mock<ISpeciesRepository>();
            _unitOfWorkMock.Setup(u => u.Species).Returns(speciesMock.Object);

            var imagesMock = new Mock<IImageRepository>();
            _unitOfWorkMock.Setup(u => u.Images).Returns(imagesMock.Object);

            _configMock.Setup(c => c.GetPathToSpeciesImageFolder(It.IsAny<int>()))
                .Returns((int id) => $"path/to/species/{id}");

            _speciesService = new SpeciesService(
                _unitOfWorkMock.Object,
                _fileServiceMock.Object,
                _configMock.Object,
                _mapper
            );
        }

        [Fact]
        public async Task AddSpecies_ShouldAddSpecies_WhenSpeciesDoesNotExist()
        {
            // Arrange
            var speciesAdd = new SpeciesAdd
            {
                Name = "Test Species",
                Description = "Test Description",
                Images = new List<IFormFile>()
            };

            _unitOfWorkMock.Setup(u => u.Species.Any(s => s.Name == speciesAdd.Name))
                .ReturnsAsync(false);

            var species = new Species
            {
                Id = 1,
                Name = speciesAdd.Name,
                Description = speciesAdd.Description,
                Images = new List<Image>(),
                Created = DateTime.Now,
                Modified = DateTime.Now
            };

            _unitOfWorkMock.Setup(u => u.Species.Add(It.IsAny<Species>()))
                .Returns(species);

            // Act
            var result = await _speciesService.AddSpecies(speciesAdd);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(species.Id, result.Id);
            Assert.Equal(species.Name, result.Name);
            Assert.Equal(species.Description, result.Description);
            _unitOfWorkMock.Verify(u => u.Species.Add(It.IsAny<Species>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task AddSpecies_ShouldReturnNull_WhenSpeciesExists()
        {
            // Arrange
            var speciesAdd = new SpeciesAdd
            {
                Name = "Test Species",
                Description = "Test Description",
                Images = new List<IFormFile>()
            };

            _unitOfWorkMock.Setup(u => u.Species.Any(s => s.Name == speciesAdd.Name))
                .ReturnsAsync(true);

            // Act
            var result = await _speciesService.AddSpecies(speciesAdd);

            // Assert
            Assert.Null(result);
            _unitOfWorkMock.Verify(u => u.Species.Add(It.IsAny<Species>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Never);
        }

        [Fact]
        public async Task DeleteSpecies_ShouldDeleteSpecies_WhenSpeciesExists()
        {
            // Arrange
            var speciesId = 1;
            var species = new Species
            {
                Id = speciesId,
                Name = "Test Species",
                Description = "Test Description",
                Images = new List<Image> { new Image() }
            };

            _unitOfWorkMock.Setup(u => u.Species.GetSpeciesWithImages(speciesId, false))
                .ReturnsAsync(species);

            // Act
            await _speciesService.DeleteSpecies(speciesId);

            // Assert
            _unitOfWorkMock.Verify(u => u.Images.Delete(It.IsAny<Image>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.Species.Delete(It.IsAny<Species>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
            _fileServiceMock.Verify(f => f.DeleteFolder(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteSpecies_ShouldNotDeleteSpecies_WhenSpeciesDoesNotExist()
        {
            // Arrange
            var speciesId = 1;

            _unitOfWorkMock.Setup(u => u.Species.GetSpeciesWithImages(speciesId, false))
                .ReturnsAsync((Species?)null);

            // Act
            await _speciesService.DeleteSpecies(speciesId);

            // Assert
            _unitOfWorkMock.Verify(u => u.Images.Delete(It.IsAny<Image>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.Species.Delete(It.IsAny<Species>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Never);
            _fileServiceMock.Verify(f => f.DeleteFolder(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetSpecies_ShouldReturnSpecies_WhenSpeciesAreFound()
        {
            // Arrange
            var search = "Test";
            var species = new List<Species>
            {
                new Species
                {
                    Id = 1,
                    Name = "Test Species",
                    Description = "Test Description",
                    Images = new List<Image> { new Image() }
                }
            };

            _unitOfWorkMock.Setup(u => u.Species.FindSpecies(search))
                .ReturnsAsync(species);

            // Act
            var result = await _speciesService.GetSpecies(search);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(species.Count, result.Count());
            Assert.Equal(species.First().Id, result.First().Id);
            Assert.Equal(species.First().Name, result.First().Name);
            Assert.Equal(species.First().Description, result.First().Description);
        }

        [Fact]
        public async Task GetSpecies_ShouldReturnEmpty_WhenNoSpeciesAreFound()
        {
            // Arrange
            var search = "Test";

            _unitOfWorkMock.Setup(u => u.Species.FindSpecies(search))
                .ReturnsAsync(new List<Species>());

            // Act
            var result = await _speciesService.GetSpecies(search);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSpeciesById_ShouldReturnSpecies_WhenSpeciesIsFound()
        {
            // Arrange
            var speciesId = 1;
            var species = new Species
            {
                Id = speciesId,
                Name = "Test Species",
                Description = "Test Description",
                Images = new List<Image> { new Image() }
            };

            _unitOfWorkMock.Setup(u => u.Species.GetSpeciesWithImages(speciesId, true))
                .ReturnsAsync(species);

            // Act
            var result = await _speciesService.GetSpeciesById(speciesId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(species.Id, result.Id);
            Assert.Equal(species.Name, result.Name);
            Assert.Equal(species.Description, result.Description);
            Assert.Equal(species.Images.Count(), result.Images.Count());
        }

        [Fact]
        public async Task GetSpeciesById_ShouldReturnNull_WhenSpeciesIsNotFound()
        {
            // Arrange
            var speciesId = 1;

            _unitOfWorkMock.Setup(u => u.Species.GetSpeciesWithImages(speciesId, true))
                .ReturnsAsync((Species?)null);

            // Act
            var result = await _speciesService.GetSpeciesById(speciesId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateSpecies_ShouldUpdateSpeciesAndImages_WhenSpeciesIsFound()
        {
            // Arrange
            var speciesId = 1;
            var speciesUpdate = new SpeciesUpdate
            {
                Id = speciesId,
                Name = "Updated Species",
                Description = "Updated Description",
                Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Data", "image.jpg") }
            };

            var species = new Species
            {
                Id = speciesId,
                Name = "Test Species",
                Description = "Test Description"
            };

            _unitOfWorkMock.Setup(u => u.Species.GetSpeciesWithImages(speciesId, false))
                .ReturnsAsync(species);

            _fileServiceMock.Setup(f => f.AddFile(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync("path/to/species/1/image.jpg");

            // Act
            var result = await _speciesService.UpdateSpecies(speciesId, speciesUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(speciesUpdate.Id, result.Id);
            Assert.Equal(speciesUpdate.Name, result.Name);
            Assert.Equal(speciesUpdate.Description, result.Description);
            Assert.Equal(speciesUpdate.Images.Count(), result.Images.Count());
            Assert.Contains(result.Images, i => i.Name == "image.jpg" && i.Path == "path/to/species/1/image.jpg");
            
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
            _fileServiceMock.Verify(f => f.DeleteFile(It.IsAny<string>()), Times.Never);
            _fileServiceMock.Verify(f => f.AddFile(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task UpdateSpecies_ShouldReturnNull_WhenSpeciesIsNotFound()
        {
            // Arrange
            var speciesId = 1;
            var speciesUpdate = new SpeciesUpdate
            {
                Name = "Updated Species",
                Description = "Updated Description",
                Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Data", "image.jpg") }
            };

            _unitOfWorkMock.Setup(u => u.Species.GetSpeciesWithImages(speciesId, false))
                .ReturnsAsync((Species?)null);

            // Act
            var result = await _speciesService.UpdateSpecies(speciesId, speciesUpdate);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateSpecies_ShouldDeleteImage_WhenImageIsNotIncludedInUpdate()
        {
            // Arrange
            var speciesId = 1;
            var speciesUpdate = new SpeciesUpdate
            {
                Id = speciesId,
                Name = "Updated Species",
                Description = "Updated Description",
                Images = new List<IFormFile>() // No images in the update
            };

            var species = new Species
            {
                Id = speciesId,
                Name = "Test Species",
                Description = "Test Description",
                Images = new List<Image> { new Image { Name = "image.jpg", Path = "path/to/species/1/image.jpg" } } // Species initially has an image
            };

            _unitOfWorkMock.Setup(u => u.Species.GetSpeciesWithImages(speciesId, false))
                .ReturnsAsync(species);

            // Act
            var result = await _speciesService.UpdateSpecies(speciesId, speciesUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(speciesUpdate.Id, result.Id);
            Assert.Equal(speciesUpdate.Name, result.Name);
            Assert.Equal(speciesUpdate.Description, result.Description);
            Assert.Empty(result.Images); // Assert that no images are associated with the species after the update
            _unitOfWorkMock.Verify(u => u.Images.Delete(It.IsAny<Image>()), Times.Once); // Verify that an image was deleted
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
            _fileServiceMock.Verify(f => f.DeleteFile(It.IsAny<string>()), Times.Once); // Verify that the image file was deleted
        }

        [Fact]
        public async Task UpdateSpecies_ShouldDeleteOneImage_WhenOneImageIsNotIncludedInUpdate()
        {
            // Arrange
            var speciesId = 1;
            var speciesUpdate = new SpeciesUpdate
            {
                Id = speciesId,
                Name = "Updated Species",
                Description = "Updated Description",
                Images = new List<IFormFile> { new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("This is a dummy file")), 0, 0, "Data", "image1.jpg") } // One existing image in the update
            };

            var species = new Species
            {
                Id = speciesId,
                Name = "Test Species",
                Description = "Test Description",
                Images = new List<Image>
                {
                    new Image { Name = "image1.jpg", Path = "path/to/species/1/image1.jpg" },
                    new Image { Name = "image2.jpg", Path = "path/to/species/1/image2.jpg" } // Species initially has two images
                }
            };

            _unitOfWorkMock.Setup(u => u.Species.GetSpeciesWithImages(speciesId, false))
                .ReturnsAsync(species);

            // Act
            var result = await _speciesService.UpdateSpecies(speciesId, speciesUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(speciesUpdate.Id, result.Id);
            Assert.Equal(speciesUpdate.Name, result.Name);
            Assert.Equal(speciesUpdate.Description, result.Description);
            Assert.Single(result.Images); // Assert that one image is associated with the species after the update
            Assert.Equal("image1.jpg", result.Images.First().Name);
            _unitOfWorkMock.Verify(u => u.Images.Delete(It.IsAny<Image>()), Times.Once); // Verify that one image was deleted
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
            _fileServiceMock.Verify(f => f.DeleteFile(It.IsAny<string>()), Times.Once); // Verify that one image file was deleted
        }

    }
}
