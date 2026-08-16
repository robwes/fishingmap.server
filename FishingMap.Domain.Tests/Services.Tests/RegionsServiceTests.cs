using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.DTO.Regions;
using FishingMap.Domain.MapsterConfig;
using FishingMap.Domain.Services;
using Mapster;
using MapsterMapper;
using Moq;
using System.Linq.Expressions;

namespace FishingMap.Domain.Tests.Services.Tests
{
    public class RegionsServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRegionRepository> _regionsRepoMock;
        private readonly IMapper _mapper;
        private readonly RegionsService _service;

        public RegionsServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _regionsRepoMock = new Mock<IRegionRepository>();
            _unitOfWorkMock.Setup(u => u.Regions).Returns(_regionsRepoMock.Object);

            var config = new TypeAdapterConfig();
            config.Scan(typeof(MapsterRegister).Assembly);
            _mapper = new Mapper(config);

            _service = new RegionsService(_unitOfWorkMock.Object, _mapper);
        }

        [Fact]
        public async Task AddRegion_ShouldThrow_WhenNationalAlreadyExists()
        {
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(true);

            var add = new RegionAdd { Name = "Finland", Type = RegionType.Root };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddRegion(add));
            Assert.Contains("root region already exists", ex.Message);
        }

        [Fact]
        public async Task AddRegion_ShouldThrow_WhenNationalHasParent()
        {
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(false);

            var add = new RegionAdd { Name = "Finland", Type = RegionType.Root, ParentRegionId = 1 };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddRegion(add));
            Assert.Contains("root region cannot have a parent", ex.Message);
        }

        [Fact]
        public async Task AddRegion_ShouldThrow_WhenNonNationalMissingParent()
        {
            var add = new RegionAdd { Name = "Uusimaa", Type = RegionType.StateRegion };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddRegion(add));
            Assert.Contains("Non-national regions must have a parent", ex.Message);
        }

        [Fact]
        public async Task AddRegion_ShouldThrow_WhenParentNotFound()
        {
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(false);

            var add = new RegionAdd { Name = "Uusimaa", Type = RegionType.StateRegion, ParentRegionId = 99 };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddRegion(add));
            Assert.Contains("Parent region with id 99 not found", ex.Message);
        }

        [Fact]
        public async Task AddRegion_ShouldPersistAndReturnDto_WhenValid()
        {
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(true);
            _regionsRepoMock.Setup(r => r.Add(It.IsAny<Region>())).Returns<Region>(r => { r.Id = 42; return r; });

            var add = new RegionAdd { Name = "Uusimaa", Type = RegionType.StateRegion, ParentRegionId = 1 };

            var dto = await _service.AddRegion(add);

            Assert.Equal(42, dto.Id);
            Assert.Equal("Uusimaa", dto.Name);
            Assert.Equal(RegionType.StateRegion, dto.Type);
            Assert.Equal(1, dto.ParentRegionId);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task UpdateRegion_ShouldThrowKeyNotFound_WhenMissing()
        {
            _regionsRepoMock.Setup(r => r.GetById(7, null, false)).ReturnsAsync((Region?)null);

            var upd = new RegionUpdate { Id = 7, Name = "x", Type = RegionType.StateRegion, ParentRegionId = 1 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateRegion(7, upd));
        }

        [Fact]
        public async Task UpdateRegion_ShouldThrow_WhenChangingNationalType()
        {
            _regionsRepoMock.Setup(r => r.GetById(1, null, false))
                .ReturnsAsync(new Region { Id = 1, Name = "Finland", Type = RegionType.Root });

            var upd = new RegionUpdate { Id = 1, Name = "Finland", Type = RegionType.StateRegion };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateRegion(1, upd));
            Assert.Contains("root region's type cannot be changed", ex.Message);
        }

        [Fact]
        public async Task UpdateRegion_ShouldThrow_WhenPromotingToNational()
        {
            _regionsRepoMock.Setup(r => r.GetById(2, null, false))
                .ReturnsAsync(new Region { Id = 2, Name = "Uusimaa", Type = RegionType.StateRegion, ParentRegionId = 1 });

            var upd = new RegionUpdate { Id = 2, Name = "Uusimaa", Type = RegionType.Root };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateRegion(2, upd));
            Assert.Contains("Cannot promote a region to Root", ex.Message);
        }

        [Fact]
        public async Task UpdateRegion_ShouldThrow_WhenSelfParent()
        {
            _regionsRepoMock.Setup(r => r.GetById(2, null, false))
                .ReturnsAsync(new Region { Id = 2, Name = "Uusimaa", Type = RegionType.StateRegion, ParentRegionId = 1 });

            var upd = new RegionUpdate { Id = 2, Name = "Uusimaa", Type = RegionType.StateRegion, ParentRegionId = 2 };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateRegion(2, upd));
            Assert.Contains("cannot be its own parent", ex.Message);
        }

        [Fact]
        public async Task UpdateRegion_ShouldThrow_WhenParentIsDescendant()
        {
            // Region 2 is the parent of region 3; moving 2 under 3 would create a cycle.
            _regionsRepoMock.Setup(r => r.GetById(2, null, false))
                .ReturnsAsync(new Region { Id = 2, Name = "Uusimaa", Type = RegionType.StateRegion, ParentRegionId = 1 });
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(true);
            _regionsRepoMock.Setup(r => r.GetAncestry(3)).ReturnsAsync(new List<Region>
            {
                new Region { Id = 3, Type = RegionType.FisheriesRegion, ParentRegionId = 2 },
                new Region { Id = 2, Type = RegionType.StateRegion, ParentRegionId = 1 },
                new Region { Id = 1, Type = RegionType.Root }
            });

            var upd = new RegionUpdate { Id = 2, Name = "Uusimaa", Type = RegionType.StateRegion, ParentRegionId = 3 };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateRegion(2, upd));
            Assert.Contains("descendants", ex.Message);
        }

        [Fact]
        public async Task UpdateRegion_ShouldPersist_WhenValid()
        {
            var entity = new Region { Id = 2, Name = "Old", Type = RegionType.StateRegion, ParentRegionId = 1 };
            _regionsRepoMock.Setup(r => r.GetById(2, null, false)).ReturnsAsync(entity);
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(true);
            _regionsRepoMock.Setup(r => r.GetAncestry(3)).ReturnsAsync(new List<Region>
            {
                new Region { Id = 3, Type = RegionType.StateRegion, ParentRegionId = 1 },
                new Region { Id = 1, Type = RegionType.Root }
            });

            var upd = new RegionUpdate { Id = 2, Name = "New", Type = RegionType.FisheriesRegion, ParentRegionId = 3 };
            var dto = await _service.UpdateRegion(2, upd);

            Assert.Equal("New", entity.Name);
            Assert.Equal(RegionType.FisheriesRegion, entity.Type);
            Assert.Equal(3, entity.ParentRegionId);
            Assert.Equal("New", dto.Name);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task DeleteRegion_ShouldThrow_WhenNational()
        {
            _regionsRepoMock.Setup(r => r.GetById(1, null, false))
                .ReturnsAsync(new Region { Id = 1, Type = RegionType.Root });

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteRegion(1));
            Assert.Contains("root region cannot be deleted", ex.Message);
        }

        [Fact]
        public async Task DeleteRegion_ShouldThrow_WhenHasChildren()
        {
            _regionsRepoMock.Setup(r => r.GetById(2, null, false))
                .ReturnsAsync(new Region { Id = 2, Type = RegionType.StateRegion, ParentRegionId = 1 });
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteRegion(2));
            Assert.Contains("Region has child regions", ex.Message);
        }

        [Fact]
        public async Task DeleteRegion_ShouldNoOp_WhenMissing()
        {
            _regionsRepoMock.Setup(r => r.GetById(99, null, false)).ReturnsAsync((Region?)null);

            await _service.DeleteRegion(99);

            _regionsRepoMock.Verify(r => r.Delete(It.IsAny<Region>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Never);
        }

        [Fact]
        public async Task DeleteRegion_ShouldPersist_WhenValid()
        {
            var entity = new Region { Id = 2, Type = RegionType.StateRegion, ParentRegionId = 1 };
            _regionsRepoMock.Setup(r => r.GetById(2, null, false)).ReturnsAsync(entity);
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(false);

            await _service.DeleteRegion(2);

            _regionsRepoMock.Verify(r => r.Delete(entity), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}
