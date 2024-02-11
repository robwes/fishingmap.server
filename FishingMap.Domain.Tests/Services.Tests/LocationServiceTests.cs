using AutoMapper;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.AutoMapperProfiles;
using FishingMap.Domain.DTO.Locations;
using FishingMap.Domain.DTO.Permits;
using FishingMap.Domain.DTO.Species;
using FishingMap.Domain.Interfaces;
using FishingMap.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Moq;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;
using Location = FishingMap.Data.Entities.Location;

namespace FishingMap.Domain.Tests.Services.Tests
{
    public class LocationServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<IFishingMapConfiguration> _configMock;
        private readonly IMapper _mapper;
        private readonly GeometryFactory _geometryFactory;
        private readonly LocationsService _locationService;

        public LocationServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _fileServiceMock = new Mock<IFileService>();
            _configMock = new Mock<IFishingMapConfiguration>();

            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DomainProfile>();
            }).CreateMapper();

            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);

            var imagesMock = new Mock<IImageRepository>();
            _unitOfWorkMock.Setup(u => u.Images).Returns(imagesMock.Object);

            var speciesMock = new Mock<ISpeciesRepository>();
            _unitOfWorkMock.Setup(u => u.Species).Returns(speciesMock.Object);

            var permitsMock = new Mock<IPermitRepository>();
            _unitOfWorkMock.Setup(u => u.Permits).Returns(permitsMock.Object);

            _configMock.Setup(c => c.GetPathToSpeciesImageFolder(It.IsAny<int>()))
                .Returns((int id) => $"path/to/locations/{id}");

            _locationService = new LocationsService(
                _unitOfWorkMock.Object, 
                _fileServiceMock.Object, 
                _configMock.Object, 
                _geometryFactory,
                _mapper);
        }

        [Fact]
        public async Task AddLocation_ShouldAddLocationAndReturnLocationDTO_WhenLocationAddIsProvided()
        {
            // Arrange
            var locationAdd = new LocationAdd
            {
                Name = "Test Location",
                Description = "Test Description",
                Rules = "Test Rules",
                WebSite = "Test Website",
                Species = new List<SpeciesDTO> { new SpeciesDTO { Id = 1, Name = "Test Species" } },
                Permits = new List<PermitDTO> { new PermitDTO { Id = 1, Name = "Test Permit", Url = "https://test.com" } },
                Geometry = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[30.0,20.0],[45.0,40.0],[10.0,40.0],[30.0,20.0]]]]},\"properties\":null}",
                Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Data", "image.jpg") }
            };

            _unitOfWorkMock.Setup(u => u.Species.GetAll(
                It.IsAny<Expression<Func<Species, bool>>>(), null, null, false))
                .ReturnsAsync(new List<Species> { new Species { Id = 1, Name = "Test Species" } });

            _unitOfWorkMock.Setup(u => u.Permits.GetAll(
                It.IsAny<Expression<Func<Permit, bool>>>(), null, null, false))
                .ReturnsAsync(new List<Permit> { new Permit { Id = 1, Name = "Test Permit", Url = "https://test.com" } });

            _fileServiceMock.Setup(f => f.AddFile(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync("path/to/locations/1/image.jpg");

            _unitOfWorkMock.Setup(u => u.Locations.Add(It.IsAny<Location>())).Returns((Location location) => location);
            _unitOfWorkMock.Setup(u => u.SaveChanges()).Returns(Task.CompletedTask);

            // Act
            var result = await _locationService.AddLocation(locationAdd);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(locationAdd.Name, result.Name);
            Assert.Equal(locationAdd.Description, result.Description);
            Assert.Equal(locationAdd.Rules, result.Rules);
            Assert.Equal(locationAdd.WebSite, result.WebSite);
            Assert.Equal(locationAdd.Species.First().Name, result.Species.First().Name);
            Assert.Equal(locationAdd.Permits.First().Name, result.Permits.First().Name);
            Assert.Equal(locationAdd.Geometry, result.Geometry);
            Assert.Equal(locationAdd.Images.First().FileName, result.Images.First().Name);
        }

        [Fact]
        public async Task DeleteLocation_LocationExists_DeletesLocation()
        {
            // Arrange
            var locationId = 1;
            var location = new Location { Id = locationId, Images = new List<Image> { new Image() } };
            _unitOfWorkMock.Setup(u => u.Locations.GetById(locationId, It.IsAny<Expression<Func<Location, object>>[]>(), false)).ReturnsAsync(location);

            // Act
            await _locationService.DeleteLocation(locationId);

            // Assert
            _unitOfWorkMock.Verify(u => u.Locations.Delete(location), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
            _fileServiceMock.Verify(f => f.DeleteFolder(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteLocation_LocationDoesNotExist_DoesNotDeleteLocation()
        {
            // Arrange
            var locationId = 1;
            _unitOfWorkMock.Setup(u => u.Locations.GetById(locationId, It.IsAny<Expression<Func<Location, object>>[]>(), false)).ReturnsAsync((Location?)null);

            // Act
            await _locationService.DeleteLocation(locationId);

            // Assert
            _unitOfWorkMock.Verify(u => u.Locations.Delete(It.IsAny<Location>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Never);
            _fileServiceMock.Verify(f => f.DeleteFolder(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetLocation_LocationExists_ReturnsLocationDTO()
        {
            // Arrange
            var locationId = 1;
            var location = new Location { Id = locationId };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, true)).ReturnsAsync(location);

            // Act
            var result = await _locationService.GetLocation(locationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(locationId, result.Id);
        }

        [Fact]
        public async Task GetLocation_LocationDoesNotExist_ReturnsNull()
        {
            // Arrange
            var locationId = 1;
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, true)).ReturnsAsync((Location)null);

            // Act
            var result = await _locationService.GetLocation(locationId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateLocation_LocationExists_ReturnsUpdatedLocationDTO()
        {
            // Arrange
            var locationId = 1;
            var locationUpdate = new LocationUpdate
            {
                Name = "Updated Location",
                Description = "Updated Description",
                Rules = "Updated Rules",
                WebSite = "Updated Website",
                Species = new List<SpeciesDTO>(),
                Permits = new List<PermitDTO>(),
                Geometry = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[30.0,20.0],[45.0,40.0],[10.0,40.0],[30.0,20.0]]]]},\"properties\":null}"
            };
            var location = new Location { Id = locationId };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);

            // Act
            var result = await _locationService.UpdateLocation(locationId, locationUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(locationUpdate.Name, result.Name);
            Assert.Equal(locationUpdate.Description, result.Description);
            Assert.Equal(locationUpdate.Rules, result.Rules);
            Assert.Equal(locationUpdate.WebSite, result.WebSite);
        }

        [Fact]
        public async Task UpdateLocation_LocationDoesNotExist_ReturnsNull()
        {
            // Arrange
            var locationId = 1;
            var locationUpdate = new LocationUpdate();
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync((Location)null);

            // Act
            var result = await _locationService.UpdateLocation(locationId, locationUpdate);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateLocation_LocationExistsAndSpeciesUpdated_ReturnsUpdatedLocationDTO()
        {
            // Arrange
            var locationId = 1;
            var locationUpdate = new LocationUpdate
            {
                Name = "Updated Location",
                Description = "Updated Description",
                Rules = "Updated Rules",
                WebSite = "Updated Website",
                Species = new List<SpeciesDTO> { new SpeciesDTO { Id = 2, Name = "Updated Species" } },
                Geometry = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[30.0,20.0],[45.0,40.0],[10.0,40.0],[30.0,20.0]]]]},\"properties\":null}"
            };
            var location = new Location { Id = locationId };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);
            _unitOfWorkMock.Setup(u => u.Species.GetAll(It.IsAny<Expression<Func<Species, bool>>>(), null, null, false)).ReturnsAsync(new List<Species> { new Species { Id = 2, Name = "Updated Species" } });

            // Act
            var result = await _locationService.UpdateLocation(locationId, locationUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(locationUpdate.Name, result.Name);
            Assert.Equal(locationUpdate.Description, result.Description);
            Assert.Equal(locationUpdate.Rules, result.Rules);
            Assert.Equal(locationUpdate.WebSite, result.WebSite);
            Assert.Equal(locationUpdate.Species.First().Name, result.Species.First().Name);
        }

        [Fact]
        public async Task UpdateLocation_LocationExistsAndSpeciesCleared_ReturnsUpdatedLocationDTO()
        {
            // Arrange
            var locationId = 1;
            var locationUpdate = new LocationUpdate
            {
                Name = "Updated Location",
                Description = "Updated Description",
                Rules = "Updated Rules",
                WebSite = "Updated Website",
                Species = null,
                Geometry = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[30.0,20.0],[45.0,40.0],[10.0,40.0],[30.0,20.0]]]]},\"properties\":null}"
            };
            var location = new Location { Id = locationId, Species = new List<Species> { new Species { Id = 1, Name = "Existing Species" } } };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);

            // Act
            var result = await _locationService.UpdateLocation(locationId, locationUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(locationUpdate.Name, result.Name);
            Assert.Equal(locationUpdate.Description, result.Description);
            Assert.Equal(locationUpdate.Rules, result.Rules);
            Assert.Equal(locationUpdate.WebSite, result.WebSite);
            Assert.Empty(result.Species);
        }

        [Fact]
        public async Task UpdateLocation_LocationExistsAndSpeciesAdded_ReturnsUpdatedLocationDTO()
        {
            // Arrange
            var locationId = 1;
            var locationUpdate = new LocationUpdate
            {
                Name = "Updated Location",
                Description = "Updated Description",
                Rules = "Updated Rules",
                WebSite = "Updated Website",
                Species = new List<SpeciesDTO> { new SpeciesDTO { Id = 1, Name = "Existing Species" }, new SpeciesDTO { Id = 2, Name = "New Species" } }
            };
            var location = new Location { Id = locationId, Species = new List<Species> { new Species { Id = 1, Name = "Existing Species" } } };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);
            _unitOfWorkMock.Setup(u => u.Species.GetAll(It.IsAny<Expression<Func<Species, bool>>>(), null, null, false)).ReturnsAsync(new List<Species> { new Species { Id = 1, Name = "Existing Species" }, new Species { Id = 2, Name = "New Species" } });

            // Act
            var result = await _locationService.UpdateLocation(locationId, locationUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(locationUpdate.Name, result.Name);
            Assert.Equal(locationUpdate.Description, result.Description);
            Assert.Equal(locationUpdate.Rules, result.Rules);
            Assert.Equal(locationUpdate.WebSite, result.WebSite);
            Assert.Equal(2, result.Species.Count());
            Assert.Contains(result.Species, s => s.Name == "New Species");
        }

        [Fact]
        public async Task UpdateLocation_LocationExistsAndSpeciesDeleted_ReturnsUpdatedLocationDTO()
        {
            // Arrange
            var locationId = 1;
            var locationUpdate = new LocationUpdate
            {
                Name = "Updated Location",
                Description = "Updated Description",
                Rules = "Updated Rules",
                WebSite = "Updated Website",
                Species = new List<SpeciesDTO> { new SpeciesDTO { Id = 1, Name = "Remaining Species" } }
            };
            var location = new Location { Id = locationId, Species = new List<Species> { new Species { Id = 1, Name = "Remaining Species" }, new Species { Id = 2, Name = "Deleted Species" } } };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);
            _unitOfWorkMock.Setup(u => u.Species.GetAll(It.IsAny<Expression<Func<Species, bool>>>(), null, null, false)).ReturnsAsync(new List<Species> { new Species { Id = 1, Name = "Remaining Species" } });

            // Act
            var result = await _locationService.UpdateLocation(locationId, locationUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(locationUpdate.Name, result.Name);
            Assert.Equal(locationUpdate.Description, result.Description);
            Assert.Equal(locationUpdate.Rules, result.Rules);
            Assert.Equal(locationUpdate.WebSite, result.WebSite);
            Assert.Single(result.Species);
            Assert.DoesNotContain(result.Species, s => s.Name == "Deleted Species");
        }

        [Fact]
        public async Task UpdateLocation_LocationExistsAndGeometryUpdated_ReturnsUpdatedLocationDTO()
        {
            // Arrange
            var locationId = 1;
            var locationUpdate = new LocationUpdate
            {
                Name = "Updated Location",
                Description = "Updated Description",
                Rules = "Updated Rules",
                WebSite = "Updated Website",
                Geometry = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[40.1,30.0],[55.0,50.0],[20.0,50.0],[40.1,30.0]]]]},\"properties\":null}"
            };
            var location = new Location { 
                Id = locationId 
            
            };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);

            // Act
            var result = await _locationService.UpdateLocation(locationId, locationUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(locationUpdate.Name, result.Name);
            Assert.Equal(locationUpdate.Description, result.Description);
            Assert.Equal(locationUpdate.Rules, result.Rules);
            Assert.Equal(locationUpdate.WebSite, result.WebSite);
            Assert.Equal(locationUpdate.Geometry, result.Geometry);
        }

        [Fact]
        public async Task UpdateLocation_LocationExistsAndImagesUpdated_ReturnsUpdatedLocationDTO()
        {
            // Arrange
            var locationId = 1;
            var locationUpdate = new LocationUpdate
            {
                Name = "Updated Location",
                Description = "Updated Description",
                Rules = "Updated Rules",
                WebSite = "Updated Website",
                Images = new List<IFormFile> { new FormFile(new MemoryStream(), 0, 0, "Data", "image2.jpg") }
            };
            var location = new Location { Id = locationId, Images = new List<Image> { new Image { Id = 1, Name = "image1.jpg", Path = "path/to/image1.jpg" } } };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);
            _fileServiceMock.Setup(f => f.AddFile(It.IsAny<IFormFile>(), It.IsAny<string>())).ReturnsAsync("path/to/image2.jpg");

            // Act
            var result = await _locationService.UpdateLocation(locationId, locationUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(locationUpdate.Name, result.Name);
            Assert.Equal(locationUpdate.Description, result.Description);
            Assert.Equal(locationUpdate.Rules, result.Rules);
            Assert.Equal(locationUpdate.WebSite, result.WebSite);
            Assert.Single(result.Images);
            Assert.Contains(result.Images, i => i.Name == "image2.jpg");
        }

        [Fact]
        public async Task UpdateLocation_LocationExistsAndImagesCleared_ReturnsUpdatedLocationDTO()
        {
            // Arrange
            var locationId = 1;
            var locationUpdate = new LocationUpdate
            {
                Name = "Updated Location",
                Description = "Updated Description",
                Rules = "Updated Rules",
                WebSite = "Updated Website",
                Images = null
            };
            var location = new Location { Id = locationId, Images = new List<Image> { new Image { Id = 1, Name = "image1.jpg", Path = "path/to/image1.jpg" } } };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);

            // Act
            var result = await _locationService.UpdateLocation(locationId, locationUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(locationUpdate.Name, result.Name);
            Assert.Equal(locationUpdate.Description, result.Description);
            Assert.Equal(locationUpdate.Rules, result.Rules);
            Assert.Equal(locationUpdate.WebSite, result.WebSite);
            Assert.Empty(result.Images);
        }


    }
}
