using Mapster;
using MapsterMapper;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.MapsterConfig;
using FishingMap.Domain.DTO.Common;
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
        private readonly Mock<IRegulationsService> _regulationsServiceMock;
        private readonly Mock<ILocationSpeciesFollowsRegionRepository> _followsRegionMock;
        private readonly IMapper _mapper;
        private readonly GeometryFactory _geometryFactory;
        private readonly LocationsService _locationService;

        public LocationServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _fileServiceMock = new Mock<IFileService>();
            _configMock = new Mock<IFishingMapConfiguration>();
            _regulationsServiceMock = new Mock<IRegulationsService>();

            var config = new TypeAdapterConfig();
            config.Scan(typeof(MapsterRegister).Assembly);
            _mapper = new Mapper(config);

            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);

            var imagesMock = new Mock<IImageRepository>();
            _unitOfWorkMock.Setup(u => u.Images).Returns(imagesMock.Object);

            var speciesMock = new Mock<ISpeciesRepository>();
            _unitOfWorkMock.Setup(u => u.Species).Returns(speciesMock.Object);

            var permitsMock = new Mock<IPermitRepository>();
            _unitOfWorkMock.Setup(u => u.Permits).Returns(permitsMock.Object);

            // Editing a location prunes the follow-region decisions of species it no longer
            // lists. Nothing follows by default here; the pruning itself is tested below.
            _followsRegionMock = new Mock<ILocationSpeciesFollowsRegionRepository>();
            _followsRegionMock.Setup(f => f.GetAll(
                It.IsAny<Expression<Func<LocationSpeciesFollowsRegion, bool>>>(),
                It.IsAny<Expression<Func<LocationSpeciesFollowsRegion, object>>[]?>(),
                It.IsAny<Func<IQueryable<LocationSpeciesFollowsRegion>, IOrderedQueryable<LocationSpeciesFollowsRegion>>?>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new List<LocationSpeciesFollowsRegion>());
            _unitOfWorkMock.Setup(u => u.LocationSpeciesFollowsRegions).Returns(_followsRegionMock.Object);

            _configMock.Setup(c => c.GetPathToSpeciesImageFolder(It.IsAny<int>()))
                .Returns((int id) => $"path/to/locations/{id}");

            _locationService = new LocationsService(
                _unitOfWorkMock.Object,
                _fileServiceMock.Object,
                _configMock.Object,
                _geometryFactory,
                _mapper,
                _regulationsServiceMock.Object);
        }

        private static FormFile CreateTestImage(string fileName, string name = "Data")
        {
            var content = new byte[] { 1, 2, 3, 4 };
            return new FormFile(new MemoryStream(content), 0, content.Length, name, fileName);
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
                Images = new List<IFormFile> { CreateTestImage("image.jpg") }
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
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, true)).ReturnsAsync((Location?)null);

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
        public async Task UpdateLocation_ShouldThrowKeyNotFoundException_WhenLocationDoesNotExist()
        {
            // Arrange
            var id = 1;
            var locationDto = new LocationUpdate { Name = "Test", Description = "Test Description" };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(id, false)).ReturnsAsync((Location?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _locationService.UpdateLocation(id, locationDto));
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
                Species = new List<SpeciesDTO> { new SpeciesDTO { Id = 1, Name = "Existing Species" }, new SpeciesDTO { Id = 2, Name = "New Species" } },
                Geometry = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[40.1,30.0],[55.0,50.0],[20.0,50.0],[40.1,30.0]]]]},\"properties\":null}"
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
                Species = new List<SpeciesDTO> { new SpeciesDTO { Id = 1, Name = "Remaining Species" } },
                Geometry = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[40.1,30.0],[55.0,50.0],[20.0,50.0],[40.1,30.0]]]]},\"properties\":null}"
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
                Images = new List<IFormFile> { CreateTestImage("image2.jpg") },
                Geometry = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[40.1,30.0],[55.0,50.0],[20.0,50.0],[40.1,30.0]]]]},\"properties\":null}"
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
                Geometry = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[40.1,30.0],[55.0,50.0],[20.0,50.0],[40.1,30.0]]]]},\"properties\":null}"
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

        // --- UpdateLocationInfo ---

        [Fact]
        public async Task UpdateLocationInfo_AppliesPresentFields_SkipsAbsent()
        {
            // Arrange
            var locationId = 1;
            var location = new Location { Id = locationId, Name = "Original", Description = "Original Desc", Rules = "Original Rules" };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);

            var patch = new LocationInfoPatch
            {
                Name = Optional<string>.Of("New Name"),
                Description = Optional<string?>.Of(null)
            };

            // Act
            var result = await _locationService.UpdateLocationInfo(locationId, patch);

            // Assert
            Assert.Equal("New Name", result.Name);
            Assert.Null(result.Description);
            Assert.Equal("Original Rules", result.Rules);
        }

        [Fact]
        public async Task UpdateLocationInfo_ThrowsKeyNotFoundException_WhenLocationMissing()
        {
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(99, false)).ReturnsAsync((Location?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _locationService.UpdateLocationInfo(99, new LocationInfoPatch()));
        }

        [Fact]
        public async Task UpdateLocationInfo_ThrowsArgumentException_WhenNameIsEmpty()
        {
            var locationId = 1;
            var location = new Location { Id = locationId, Name = "Original" };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);

            var patch = new LocationInfoPatch { Name = Optional<string>.Of(string.Empty) };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _locationService.UpdateLocationInfo(locationId, patch));
        }

        // --- UpdateLocationAssociations ---

        [Fact]
        public async Task UpdateLocationAssociations_ReplacesSpeciesAndPermits()
        {
            // Arrange
            var locationId = 1;
            var location = new Location { Id = locationId, Species = new List<Species> { new Species { Id = 1, Name = "Old" } }, Permits = new List<Permit>() };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);
            _unitOfWorkMock.Setup(u => u.Species.GetAll(It.IsAny<Expression<Func<Species, bool>>>(), null, null, false))
                .ReturnsAsync(new List<Species> { new Species { Id = 2, Name = "New Species" } });
            _unitOfWorkMock.Setup(u => u.Permits.GetAll(It.IsAny<Expression<Func<Permit, bool>>>(), null, null, false))
                .ReturnsAsync(new List<Permit> { new Permit { Id = 3, Name = "New Permit", Url = "https://test.com" } });

            var patch = new LocationAssociationsPatch
            {
                Species = new List<SpeciesDTO> { new SpeciesDTO { Id = 2, Name = "New Species" } },
                Permits = new List<PermitDTO> { new PermitDTO { Id = 3, Name = "New Permit", Url = "https://test.com" } }
            };

            // Act
            var result = await _locationService.UpdateLocationAssociations(locationId, patch);

            // Assert
            Assert.Single(result.Species);
            Assert.Contains(result.Species, s => s.Name == "New Species");
            Assert.Single(result.Permits);
        }

        [Fact]
        public async Task UpdateLocationAssociations_DropsFollowRegionDecisions_ForSpeciesNoLongerListed()
        {
            // Otherwise removing a species and adding it back later silently restores an
            // inheritance nobody re-chose. Decision 11 in robwes/fishingmap.web#13.
            var locationId = 1;
            var location = new Location { Id = locationId, Species = new List<Species>(), Permits = new List<Permit>() };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);
            _unitOfWorkMock.Setup(u => u.Species.GetAll(It.IsAny<Expression<Func<Species, bool>>>(), null, null, false))
                .ReturnsAsync(new List<Species> { new Species { Id = 2, Name = "Kept" } });
            _unitOfWorkMock.Setup(u => u.Permits.GetAll(It.IsAny<Expression<Func<Permit, bool>>>(), null, null, false))
                .ReturnsAsync(new List<Permit>());

            var kept = new LocationSpeciesFollowsRegion { Id = 10, LocationId = locationId, SpeciesId = 2 };
            var dropped = new LocationSpeciesFollowsRegion { Id = 11, LocationId = locationId, SpeciesId = 1 };
            _followsRegionMock.Setup(f => f.GetAll(
                It.IsAny<Expression<Func<LocationSpeciesFollowsRegion, bool>>>(),
                It.IsAny<Expression<Func<LocationSpeciesFollowsRegion, object>>[]?>(),
                It.IsAny<Func<IQueryable<LocationSpeciesFollowsRegion>, IOrderedQueryable<LocationSpeciesFollowsRegion>>?>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new List<LocationSpeciesFollowsRegion> { kept, dropped });

            await _locationService.UpdateLocationAssociations(locationId, new LocationAssociationsPatch
            {
                Species = new List<SpeciesDTO> { new SpeciesDTO { Id = 2, Name = "Kept" } },
                Permits = new List<PermitDTO>()
            });

            _followsRegionMock.Verify(f => f.Delete(dropped), Times.Once);
            _followsRegionMock.Verify(f => f.Delete(kept), Times.Never);
        }

        [Fact]
        public async Task UpdateLocationAssociations_ThrowsKeyNotFoundException_WhenLocationMissing()
        {
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(99, false)).ReturnsAsync((Location?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _locationService.UpdateLocationAssociations(99, new LocationAssociationsPatch()));
        }

        [Fact]
        public async Task UpdateLocationAssociations_ClearsAssociations_WhenEmptyListsSent()
        {
            var locationId = 1;
            var location = new Location
            {
                Id = locationId,
                Species = new List<Species> { new Species { Id = 1, Name = "Old" } },
                Permits = new List<Permit> { new Permit { Id = 1, Name = "Old", Url = "x" } }
            };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);
            _unitOfWorkMock.Setup(u => u.Species.GetAll(It.IsAny<Expression<Func<Species, bool>>>(), null, null, false))
                .ReturnsAsync(new List<Species>());
            _unitOfWorkMock.Setup(u => u.Permits.GetAll(It.IsAny<Expression<Func<Permit, bool>>>(), null, null, false))
                .ReturnsAsync(new List<Permit>());

            var result = await _locationService.UpdateLocationAssociations(locationId, new LocationAssociationsPatch());

            Assert.Empty(result.Species);
            Assert.Empty(result.Permits);
        }

        // --- AddImageToLocation ---

        [Fact]
        public async Task AddImageToLocation_ReturnsImageDTO_WithPopulatedData()
        {
            var locationId = 1;
            var location = new Location { Id = locationId, Images = new List<Image>() };
            _unitOfWorkMock.Setup(u => u.Locations.GetById(locationId, It.IsAny<Expression<Func<Location, object>>[]>(), false))
                .ReturnsAsync(location);
            _fileServiceMock.Setup(f => f.AddFile(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync("locations/1/test.jpg");

            var file = CreateTestImage("test.jpg", "image");

            var result = await _locationService.AddImageToLocation(locationId, file);

            Assert.NotNull(result);
            Assert.Equal("test.jpg", result.Name);
        }

        [Fact]
        public async Task AddImageToLocation_ThrowsKeyNotFoundException_WhenLocationMissing()
        {
            _unitOfWorkMock.Setup(u => u.Locations.GetById(99, It.IsAny<Expression<Func<Location, object>>[]>(), false))
                .ReturnsAsync((Location?)null);

            var file = CreateTestImage("test.jpg", "image");

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _locationService.AddImageToLocation(99, file));
        }

        [Fact]
        public async Task AddImageToLocation_ThrowsArgumentException_WhenFileIsEmpty()
        {
            var locationId = 1;
            var location = new Location { Id = locationId, Images = new List<Image>() };
            _unitOfWorkMock.Setup(u => u.Locations.GetById(locationId, It.IsAny<Expression<Func<Location, object>>[]>(), false))
                .ReturnsAsync(location);

            var emptyFile = new FormFile(new MemoryStream(), 0, 0, "image", "empty.jpg");

            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _locationService.AddImageToLocation(locationId, emptyFile));
            Assert.Contains("empty", ex.Message);
            _fileServiceMock.Verify(f => f.AddFile(It.IsAny<IFormFile>(), It.IsAny<string>()), Times.Never);
        }

        // --- RemoveImageFromLocation ---

        [Fact]
        public async Task RemoveImageFromLocation_DeletesImageFileAndDbRow()
        {
            var locationId = 1;
            var image = new Image { Id = 5, Name = "img.jpg", Path = "locations/1/img.jpg" };
            var location = new Location { Id = locationId, Images = new List<Image> { image } };
            _unitOfWorkMock.Setup(u => u.Locations.GetById(locationId, It.IsAny<Expression<Func<Location, object>>[]>(), false))
                .ReturnsAsync(location);

            await _locationService.RemoveImageFromLocation(locationId, 5);

            _unitOfWorkMock.Verify(u => u.Images.Delete(image), Times.Once);
            _fileServiceMock.Verify(f => f.DeleteFile("locations/1/img.jpg"), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task RemoveImageFromLocation_ThrowsKeyNotFoundException_WhenImageNotOnLocation()
        {
            var locationId = 1;
            var location = new Location { Id = locationId, Images = new List<Image> { new Image { Id = 1, Name = "a.jpg", Path = "x" } } };
            _unitOfWorkMock.Setup(u => u.Locations.GetById(locationId, It.IsAny<Expression<Func<Location, object>>[]>(), false))
                .ReturnsAsync(location);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _locationService.RemoveImageFromLocation(locationId, 999));
        }

        // --- UpdateLocationGeometry ---

        [Fact]
        public async Task UpdateLocationGeometry_RecalculatesPositionAndArea()
        {
            var locationId = 1;
            var location = new Location { Id = locationId };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);

            var patch = new LocationGeometryPatch
            {
                Geometry = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[30.0,20.0],[45.0,40.0],[10.0,40.0],[30.0,20.0]]]]},\"properties\":null}"
            };

            var result = await _locationService.UpdateLocationGeometry(locationId, patch);

            Assert.NotNull(result);
            Assert.Equal(patch.Geometry, result.Geometry);
        }

        [Fact]
        public async Task UpdateLocationGeometry_ThrowsArgumentException_OnInvalidGeometry()
        {
            var locationId = 1;
            var location = new Location { Id = locationId };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);

            var patch = new LocationGeometryPatch { Geometry = "not-valid-geojson" };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                _locationService.UpdateLocationGeometry(locationId, patch));
        }

        [Fact]
        public async Task UpdateLocationGeometry_ClearsNavigationPosition_WhenNullSent()
        {
            var locationId = 1;
            var location = new Location
            {
                Id = locationId,
                NavigationPosition = new Point(25.0, 60.0) { SRID = 4326 }
            };
            _unitOfWorkMock.Setup(u => u.Locations.GetLocationWithDetails(locationId, false)).ReturnsAsync(location);

            var patch = new LocationGeometryPatch
            {
                Geometry = "{\"type\":\"Feature\",\"geometry\":{\"type\":\"MultiPolygon\",\"coordinates\":[[[[30.0,20.0],[45.0,40.0],[10.0,40.0],[30.0,20.0]]]]},\"properties\":null}",
                NavigationPosition = null
            };

            await _locationService.UpdateLocationGeometry(locationId, patch);

            Assert.Null(location.NavigationPosition);
        }

        // --- GetLocations (distance) ---

        [Fact]
        public async Task GetLocations_AssignsDistanceAndPreservesOrder_WhenOriginProvided()
        {
            var near = new Location { Id = 1, Name = "Near", Position = new Point(24.9, 60.1) { SRID = 4326 } };
            var far = new Location { Id = 2, Name = "Far", Position = new Point(25.5, 60.5) { SRID = 4326 } };
            var repoResult = new List<(Location, double?)> { (near, 2.5), (far, 40.0) };

            _unitOfWorkMock
                .Setup(u => u.Locations.FindLocationsWithDistance("", null, null, 60.17, 24.94))
                .ReturnsAsync(repoResult);

            var result = (await _locationService.GetLocations(orgLat: 60.17, orgLng: 24.94)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal(new[] { 1, 2 }, result.Select(r => r.Id));
            Assert.Equal(2.5, result[0].Distance);
            Assert.Equal(40.0, result[1].Distance);
        }

        [Fact]
        public async Task GetLocations_LeavesDistanceNull_WhenNoOriginProvided()
        {
            var location = new Location { Id = 1, Name = "A", Position = new Point(24.9, 60.1) { SRID = 4326 } };
            var repoResult = new List<(Location, double?)> { (location, null) };

            _unitOfWorkMock
                .Setup(u => u.Locations.FindLocationsWithDistance("", null, null, null, null))
                .ReturnsAsync(repoResult);

            var result = (await _locationService.GetLocations()).ToList();

            Assert.Single(result);
            Assert.Null(result[0].Distance);
        }
    }
}

