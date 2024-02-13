using AutoMapper;
using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.AutoMapperProfiles;
using FishingMap.Domain.DTO.Permits;
using FishingMap.Domain.Services;
using Moq;

namespace FishingMap.Domain.Tests.Services.Tests
{
    public class PermitServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private IMapper _mapper;
        private PermitsService _service;

        public PermitServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DomainProfile>();
            }).CreateMapper();

            _service = new PermitsService(_unitOfWorkMock.Object, _mapper);
        }

        [Fact]
        public async Task AddPermit_ShouldAddPermitAndReturnPermitDto()
        {
            // Arrange
            var permitDto = new PermitDTO { Name = "Test", Url = "http://test.com" };
            var permit = new Permit { Name = "Test", Url = "http://test.com", Created = DateTime.Now, Modified = DateTime.Now };
            _unitOfWorkMock.Setup(u => u.Permits.Add(It.IsAny<Permit>())).Returns(permit);

            // Act
            var result = await _service.AddPermit(permitDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(permitDto.Name, result.Name);
            Assert.Equal(permitDto.Url, result.Url);
            _unitOfWorkMock.Verify(u => u.Permits.Add(It.IsAny<Permit>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task AddPermit_ShouldThrowException_WhenErrorOccurs()
        {
            // Arrange
            var permitDto = new PermitDTO { Name = "Test", Url = "http://test.com" };
            _unitOfWorkMock.Setup(u => u.Permits.Add(It.IsAny<Permit>())).Throws<Exception>();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.AddPermit(permitDto));
        }

        [Fact]
        public async Task AddPermit_ShouldCorrectlyMapPermitToPermitDto()
        {
            // Arrange
            var permitDto = new PermitDTO { Name = "Test", Url = "http://test.com" };
            var permit = new Permit { Name = "Test", Url = "http://test.com", Created = DateTime.Now, Modified = DateTime.Now };
            _unitOfWorkMock.Setup(u => u.Permits.Add(It.IsAny<Permit>())).Returns(permit);

            // Act
            var result = await _service.AddPermit(permitDto);

            // Assert
            Assert.Equal(permitDto.Name, result.Name);
            Assert.Equal(permitDto.Url, result.Url);
        }

        [Fact]
        public async Task DeletePermit_ShouldDeletePermit()
        {
            // Arrange
            var id = 1;
            _unitOfWorkMock.Setup(u => u.Permits.Delete(id)).Returns(Task.CompletedTask);

            // Act
            await _service.DeletePermit(id);

            // Assert
            _unitOfWorkMock.Verify(u => u.Permits.Delete(id), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task DeletePermit_ShouldThrowException_WhenErrorOccurs()
        {
            // Arrange
            var id = 1;
            _unitOfWorkMock.Setup(u => u.Permits.Delete(id)).Throws<Exception>();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.DeletePermit(id));
        }

        [Fact]
        public async Task GetPermit_ShouldReturnPermitDto_WhenPermitExists()
        {
            // Arrange
            var id = 1;
            var permit = new Permit { Name = "Test", Url = "http://test.com", Created = DateTime.Now, Modified = DateTime.Now };
            _unitOfWorkMock.Setup(u => u.Permits.GetById(id, null, true)).ReturnsAsync(permit);

            // Act
            var result = await _service.GetPermit(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(permit.Name, result.Name);
            Assert.Equal(permit.Url, result.Url);
        }

        [Fact]
        public async Task GetPermit_ShouldReturnNull_WhenPermitDoesNotExist()
        {
            // Arrange
            var id = 1;
            _unitOfWorkMock.Setup(u => u.Permits.GetById(id, null, true)).ReturnsAsync((Permit?)null);

            // Act
            var result = await _service.GetPermit(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPermit_ShouldThrowException_WhenErrorOccurs()
        {
            // Arrange
            var id = 1;
            _unitOfWorkMock.Setup(u => u.Permits.GetById(id, null, true)).Throws<Exception>();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetPermit(id));
        }

        [Fact]
        public async Task GetPermits_ShouldReturnPermitDtos_WhenPermitsExist()
        {
            // Arrange
            var search = "Test";
            var permits = new List<Permit> { new Permit { Name = "Test", Url = "http://test.com", Created = DateTime.Now, Modified = DateTime.Now } };
            _unitOfWorkMock.Setup(u => u.Permits.FindPermits(search)).ReturnsAsync(permits);

            // Act
            var result = await _service.GetPermits(search);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(permits.Count, result.Count());
            Assert.Equal(permits.First().Name, result.First().Name);
            Assert.Equal(permits.First().Url, result.First().Url);
        }

        [Fact]
        public async Task GetPermits_ShouldReturnEmptyList_WhenNoPermitsExist()
        {
            // Arrange
            var search = "Test";
            _unitOfWorkMock.Setup(u => u.Permits.FindPermits(search)).ReturnsAsync(new List<Permit>());

            // Act
            var result = await _service.GetPermits(search);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPermits_ShouldThrowException_WhenErrorOccurs()
        {
            // Arrange
            var search = "Test";
            _unitOfWorkMock.Setup(u => u.Permits.FindPermits(search)).Throws<Exception>();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetPermits(search));
        }

        [Fact]
        public async Task UpdatePermit_ShouldUpdatePermitAndReturnPermitDto_WhenPermitExists()
        {
            // Arrange
            var id = 1;
            var permitDto = new PermitDTO { Name = "New name", Url = "http://newurl.com" };
            var permit = new Permit { Name = "Test", Url = "http://test.com", Created = DateTime.Now, Modified = DateTime.Now };
            
            _unitOfWorkMock.Setup(u => u.Permits.GetById(id, null, false)).ReturnsAsync(permit);

            // Act
            var result = await _service.UpdatePermit(id, permitDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(permitDto.Name, result.Name);
            Assert.Equal(permitDto.Url, result.Url);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task UpdatePermit_ShouldReturnNull_WhenPermitDoesNotExist()
        {
            // Arrange
            var id = 1;
            var permitDto = new PermitDTO { Name = "Test", Url = "http://test.com" };
            _unitOfWorkMock.Setup(u => u.Permits.GetById(id, null, false)).ReturnsAsync((Permit?)null);

            // Act
            var result = await _service.UpdatePermit(id, permitDto);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePermit_ShouldThrowException_WhenErrorOccurs()
        {
            // Arrange
            var id = 1;
            var permitDto = new PermitDTO { Name = "Test", Url = "http://test.com" };
            _unitOfWorkMock.Setup(u => u.Permits.GetById(id, null, false)).Throws<Exception>();

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.UpdatePermit(id, permitDto));
        }
    }
}
