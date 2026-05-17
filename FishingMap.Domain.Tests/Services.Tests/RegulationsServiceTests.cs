using FishingMap.Data.Entities;
using FishingMap.Data.Interfaces;
using FishingMap.Domain.DTO.Regulations;
using FishingMap.Domain.DTO.Species;
using FishingMap.Domain.MapsterConfig;
using FishingMap.Domain.Services;
using Mapster;
using MapsterMapper;
using Moq;
using System.Linq.Expressions;

namespace FishingMap.Domain.Tests.Services.Tests
{
    public class RegulationsServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRegionRepository> _regionsRepoMock;
        private readonly Mock<ISpeciesRegulationRepository> _regulationsRepoMock;
        private readonly Mock<ISpeciesRepository> _speciesRepoMock;
        private readonly Mock<ILocationRepository> _locationsRepoMock;
        private readonly IMapper _mapper;
        private readonly RegulationsService _service;

        public RegulationsServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _regionsRepoMock = new Mock<IRegionRepository>();
            _regulationsRepoMock = new Mock<ISpeciesRegulationRepository>();
            _speciesRepoMock = new Mock<ISpeciesRepository>();
            _locationsRepoMock = new Mock<ILocationRepository>();

            _unitOfWorkMock.Setup(u => u.Regions).Returns(_regionsRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.SpeciesRegulations).Returns(_regulationsRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Species).Returns(_speciesRepoMock.Object);
            _unitOfWorkMock.Setup(u => u.Locations).Returns(_locationsRepoMock.Object);

            var config = new TypeAdapterConfig();
            config.Scan(typeof(MapsterRegister).Assembly);
            _mapper = new Mapper(config);

            _service = new RegulationsService(_unitOfWorkMock.Object, _mapper);
        }

        [Fact]
        public async Task AddRegulation_ShouldThrow_WhenBothRegionAndLocationsSet()
        {
            var add = new SpeciesRegulationAdd { SpeciesId = 1, RegionId = 1, LocationIds = new[] { 5 } };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddRegulation(add));
            Assert.Contains("cannot have both", ex.Message);
        }

        [Fact]
        public async Task AddRegulation_ShouldThrow_WhenNeitherRegionNorLocationsSet()
        {
            var add = new SpeciesRegulationAdd { SpeciesId = 1 };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddRegulation(add));
            Assert.Contains("must specify either", ex.Message);
        }

        [Fact]
        public async Task AddRegulation_ShouldThrow_WhenRegionNotFound()
        {
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(false);

            var add = new SpeciesRegulationAdd { SpeciesId = 1, RegionId = 99 };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddRegulation(add));
            Assert.Contains("Region with id 99 not found", ex.Message);
        }

        [Fact]
        public async Task AddRegulation_ShouldThrow_WhenSpeciesNotFound()
        {
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(true);
            _speciesRepoMock.Setup(s => s.Any(It.IsAny<Expression<Func<Species, bool>>>())).ReturnsAsync(false);

            var add = new SpeciesRegulationAdd { SpeciesId = 99, RegionId = 1 };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddRegulation(add));
            Assert.Contains("Species with id 99 not found", ex.Message);
        }

        [Fact]
        public async Task AddRegulation_ShouldThrow_WhenLocationsAlreadyHaveRuleForSameSpecies()
        {
            _regulationsRepoMock.Setup(r => r.Find(
                It.IsAny<Expression<Func<SpeciesRegulation, bool>>>(),
                It.IsAny<Expression<Func<SpeciesRegulation, object>>[]?>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new SpeciesRegulation { Id = 100, SpeciesId = 1 });

            var add = new SpeciesRegulationAdd { SpeciesId = 1, LocationIds = new[] { 5 } };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddRegulation(add));
            Assert.Contains("already have a location-scoped rule", ex.Message);
        }

        [Fact]
        public async Task AddRegulation_ShouldPersistWithProtectedPeriods_WhenRegionScoped()
        {
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(true);
            _speciesRepoMock.Setup(s => s.Any(It.IsAny<Expression<Func<Species, bool>>>())).ReturnsAsync(true);

            SpeciesRegulation? captured = null;
            _regulationsRepoMock.Setup(r => r.Add(It.IsAny<SpeciesRegulation>()))
                .Returns<SpeciesRegulation>(r => { r.Id = 7; captured = r; return r; });

            var add = new SpeciesRegulationAdd
            {
                SpeciesId = 1,
                RegionId = 1,
                MinimumSizeCm = 30,
                BagLimit = 5,
                IsCatchAndReleaseOnly = false,
                MustReportCatch = true,
                AdditionalRules = "test",
                ProtectedPeriods = new[]
                {
                    new ProtectedPeriodDTO { StartMonth = 4, StartDay = 1, EndMonth = 5, EndDay = 31 }
                }
            };

            var dto = await _service.AddRegulation(add);

            Assert.Equal(7, dto.Id);
            Assert.Equal(1, dto.RegionId);
            Assert.Empty(dto.LocationIds);
            Assert.Single(dto.ProtectedPeriods);
            Assert.NotNull(captured);
            Assert.Single(captured!.ProtectedPeriods);
            _unitOfWorkMock.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task AddRegulation_ShouldAttachLocations_WhenLocationScoped()
        {
            _speciesRepoMock.Setup(s => s.Any(It.IsAny<Expression<Func<Species, bool>>>())).ReturnsAsync(true);
            _locationsRepoMock.Setup(l => l.GetAll(
                It.IsAny<Expression<Func<Location, bool>>>(), null, null, false))
                .ReturnsAsync(new List<Location>
                {
                    new() { Id = 5, Name = "A" },
                    new() { Id = 6, Name = "B" }
                });
            _regulationsRepoMock.Setup(r => r.Add(It.IsAny<SpeciesRegulation>()))
                .Returns<SpeciesRegulation>(r => { r.Id = 8; return r; });

            var add = new SpeciesRegulationAdd
            {
                SpeciesId = 1,
                LocationIds = new[] { 5, 6 }
            };

            var dto = await _service.AddRegulation(add);

            Assert.Null(dto.RegionId);
            Assert.Equal(new[] { 5, 6 }, dto.LocationIds);
        }

        [Fact]
        public async Task UpdateRegulation_ShouldThrowKeyNotFound_WhenMissing()
        {
            _regulationsRepoMock.Setup(r => r.GetById(
                It.IsAny<int>(),
                It.IsAny<Expression<Func<SpeciesRegulation, object>>[]?>(),
                It.IsAny<bool>()))
                .ReturnsAsync((SpeciesRegulation?)null);

            var upd = new SpeciesRegulationUpdate { Id = 7, SpeciesId = 1, RegionId = 1 };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateRegulation(7, upd));
        }

        [Fact]
        public async Task UpdateRegulation_ShouldReplaceLocationsAndProtectedPeriods()
        {
            var existing = new SpeciesRegulation
            {
                Id = 3,
                SpeciesId = 1,
                RegionId = null,
                Locations = new List<Location> { new() { Id = 5 } },
                ProtectedPeriods = new List<ProtectedPeriod>
                {
                    new() { Id = 11, StartMonth = 1, StartDay = 1, EndMonth = 2, EndDay = 1 }
                }
            };

            _regulationsRepoMock.Setup(r => r.GetById(
                3,
                It.IsAny<Expression<Func<SpeciesRegulation, object>>[]?>(),
                It.IsAny<bool>()))
                .ReturnsAsync(existing);
            _locationsRepoMock.Setup(l => l.GetAll(
                It.IsAny<Expression<Func<Location, bool>>>(), null, null, false))
                .ReturnsAsync(new List<Location> { new() { Id = 8 } });

            var upd = new SpeciesRegulationUpdate
            {
                Id = 3,
                SpeciesId = 1,
                LocationIds = new[] { 8 },
                ProtectedPeriods = new[]
                {
                    new ProtectedPeriodDTO { StartMonth = 6, StartDay = 1, EndMonth = 7, EndDay = 1 }
                }
            };

            var dto = await _service.UpdateRegulation(3, upd);

            Assert.Single(existing.Locations);
            Assert.Equal(8, existing.Locations.First().Id);
            Assert.Single(existing.ProtectedPeriods);
            Assert.Equal(6, existing.ProtectedPeriods.First().StartMonth);
            Assert.Equal(new[] { 8 }, dto.LocationIds);
        }

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldThrow_WhenLocationMissing()
        {
            _locationsRepoMock.Setup(l => l.GetById(99, null, true)).ReturnsAsync((Location?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetEffectiveRulesForLocation(99));
        }

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldPreferLocationScopedOverRegion()
        {
            var location = new Location { Id = 5, RegionId = 10 };
            _locationsRepoMock.Setup(l => l.GetById(5, null, true)).ReturnsAsync(location);
            _regionsRepoMock.Setup(r => r.GetAncestry(10)).ReturnsAsync(new List<Region>
            {
                new() { Id = 10, Name = "Sub", Type = RegionType.ManagementArea, ParentRegionId = 20 },
                new() { Id = 20, Name = "Ely", Type = RegionType.Ely, ParentRegionId = 1 },
                new() { Id = 1, Name = "Finland", Type = RegionType.National }
            });

            var locScoped = new SpeciesRegulation
            {
                Id = 1,
                SpeciesId = 100,
                Locations = new List<Location> { location },
                MinimumSizeCm = 50
            };
            var regionScoped = new SpeciesRegulation
            {
                Id = 2,
                SpeciesId = 100,
                RegionId = 10,
                Region = new Region { Id = 10, Type = RegionType.ManagementArea, Name = "Sub" },
                MinimumSizeCm = 30
            };

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation> { regionScoped, locScoped });

            var rules = (await _service.GetEffectiveRulesForLocation(5)).ToList();

            Assert.Single(rules);
            Assert.Equal(50, rules[0].MinimumSizeCm);
            Assert.Equal("Location", rules[0].Source);
        }

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldPreferNearestAncestor()
        {
            var location = new Location { Id = 5, RegionId = 10 };
            _locationsRepoMock.Setup(l => l.GetById(5, null, true)).ReturnsAsync(location);
            _regionsRepoMock.Setup(r => r.GetAncestry(10)).ReturnsAsync(new List<Region>
            {
                new() { Id = 10, Name = "Sub", Type = RegionType.ManagementArea, ParentRegionId = 20 },
                new() { Id = 20, Name = "Ely", Type = RegionType.Ely, ParentRegionId = 1 },
                new() { Id = 1, Name = "Finland", Type = RegionType.National }
            });

            var nationalRule = new SpeciesRegulation
            {
                Id = 1,
                SpeciesId = 100,
                RegionId = 1,
                Region = new Region { Id = 1, Type = RegionType.National, Name = "Finland" },
                MinimumSizeCm = 20
            };
            var elyRule = new SpeciesRegulation
            {
                Id = 2,
                SpeciesId = 100,
                RegionId = 20,
                Region = new Region { Id = 20, Type = RegionType.Ely, Name = "Ely" },
                MinimumSizeCm = 35
            };

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation> { nationalRule, elyRule });

            var rules = (await _service.GetEffectiveRulesForLocation(5)).ToList();

            Assert.Single(rules);
            Assert.Equal(35, rules[0].MinimumSizeCm);
            Assert.Equal("Region: Ely", rules[0].Source);
        }

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldFallBackToNational_WhenLocationHasNoRegion()
        {
            var location = new Location { Id = 5, RegionId = null };
            _locationsRepoMock.Setup(l => l.GetById(5, null, true)).ReturnsAsync(location);
            _regionsRepoMock.Setup(r => r.GetNationalRegionId()).ReturnsAsync(1);

            var nationalRule = new SpeciesRegulation
            {
                Id = 1,
                SpeciesId = 100,
                RegionId = 1,
                Region = new Region { Id = 1, Type = RegionType.National, Name = "Finland" },
                MinimumSizeCm = 20
            };

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation> { nationalRule });

            var rules = (await _service.GetEffectiveRulesForLocation(5)).ToList();

            Assert.Single(rules);
            Assert.Equal("National", rules[0].Source);
            _regionsRepoMock.Verify(r => r.GetNationalRegionId(), Times.Once);
            _regionsRepoMock.Verify(r => r.GetAncestry(It.IsAny<int>()), Times.Never);
        }
    }
}
