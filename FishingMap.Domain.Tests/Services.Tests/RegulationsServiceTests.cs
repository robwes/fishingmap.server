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

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldCarryRegulationIdAndLocationIds()
        {
            // The location editor saves against RegulationId and uses LocationIds to detect a
            // rule shared with other waters. Neither is derivable from the rest of the payload.
            var location = new Location { Id = 5, RegionId = null };
            var otherWater = new Location { Id = 7 };
            _locationsRepoMock.Setup(l => l.GetById(5, null, true)).ReturnsAsync(location);
            _regionsRepoMock.Setup(r => r.GetNationalRegionId()).ReturnsAsync(1);

            var shared = new SpeciesRegulation
            {
                Id = 42,
                SpeciesId = 100,
                Locations = new List<Location> { location, otherWater },
                MinimumSizeCm = 50
            };

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation> { shared });

            var rules = (await _service.GetEffectiveRulesForLocation(5)).ToList();

            Assert.Single(rules);
            Assert.Equal(42, rules[0].RegulationId);
            Assert.Equal(new[] { 5, 7 }, rules[0].LocationIds);
        }

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldExposeWhatTheWinnerShadows()
        {
            var location = new Location { Id = 5, RegionId = 20 };
            _locationsRepoMock.Setup(l => l.GetById(5, null, true)).ReturnsAsync(location);
            _regionsRepoMock.Setup(r => r.GetAncestry(20)).ReturnsAsync(new List<Region>
            {
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
            var locScoped = new SpeciesRegulation
            {
                Id = 2,
                SpeciesId = 100,
                Locations = new List<Location> { location },
                MinimumSizeCm = 60
            };

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation> { nationalRule, locScoped });

            var rules = (await _service.GetEffectiveRulesForLocation(5)).ToList();

            var rule = Assert.Single(rules);
            Assert.Equal("Location", rule.Source);
            Assert.Equal(60, rule.MinimumSizeCm);

            // Removing the local override would reveal the national rule — the editor says so.
            Assert.NotNull(rule.FallsBackTo);
            Assert.Equal("National", rule.FallsBackTo!.Source);
            Assert.Equal(20, rule.FallsBackTo.MinimumSizeCm);
            // Only one level deep, so the nested rule doesn't recurse.
            Assert.Null(rule.FallsBackTo.FallsBackTo);
        }

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldLeaveFallbackNull_WhenNothingElseCovers()
        {
            var location = new Location { Id = 5, RegionId = null };
            _locationsRepoMock.Setup(l => l.GetById(5, null, true)).ReturnsAsync(location);
            _regionsRepoMock.Setup(r => r.GetNationalRegionId()).ReturnsAsync(1);

            var only = new SpeciesRegulation
            {
                Id = 1,
                SpeciesId = 100,
                RegionId = 1,
                Region = new Region { Id = 1, Type = RegionType.National, Name = "Finland" }
            };

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation> { only });

            var rules = (await _service.GetEffectiveRulesForLocation(5)).ToList();

            Assert.Null(rules[0].FallsBackTo);
        }

        [Theory]
        [InlineData(2, 30)]  // February has 29 at most
        [InlineData(2, 31)]
        [InlineData(4, 31)]  // 30-day months
        [InlineData(6, 31)]
        [InlineData(9, 31)]
        [InlineData(11, 31)]
        public async Task AddRegulation_ShouldThrow_WhenPeriodNamesADayItsMonthDoesNotHave(int month, int day)
        {
            // Scope is validated first, so the region has to exist for the
            // period check to be the thing that fails.
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(true);

            // [Range(1, 31)] on the DTO passes 31 February quite happily.
            var add = new SpeciesRegulationAdd
            {
                SpeciesId = 1,
                RegionId = 1,
                ProtectedPeriods = [new ProtectedPeriodDTO
                {
                    StartMonth = month, StartDay = day, EndMonth = 12, EndDay = 31
                }]
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddRegulation(add));
            Assert.Contains("not a valid day of month", ex.Message);
        }

        [Fact]
        public async Task AddRegulation_ShouldAcceptTheEndOfFebruary()
        {
            // Periods carry no year, so "end of February" has to be expressible.
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(true);
            _speciesRepoMock.Setup(s => s.Any(It.IsAny<Expression<Func<Species, bool>>>())).ReturnsAsync(true);
            _regulationsRepoMock.Setup(r => r.Add(It.IsAny<SpeciesRegulation>()))
                .Returns((SpeciesRegulation r) => r);

            var add = new SpeciesRegulationAdd
            {
                SpeciesId = 1,
                RegionId = 1,
                ProtectedPeriods = [new ProtectedPeriodDTO
                {
                    StartMonth = 1, StartDay = 1, EndMonth = 2, EndDay = 29
                }]
            };

            var result = await _service.AddRegulation(add);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task AddRegulation_ShouldAcceptAPeriodThatWrapsPastNewYear()
        {
            // A start after its end is a winter closure, not invalid input.
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(true);
            _speciesRepoMock.Setup(s => s.Any(It.IsAny<Expression<Func<Species, bool>>>())).ReturnsAsync(true);
            _regulationsRepoMock.Setup(r => r.Add(It.IsAny<SpeciesRegulation>()))
                .Returns((SpeciesRegulation r) => r);

            var add = new SpeciesRegulationAdd
            {
                SpeciesId = 1,
                RegionId = 1,
                ProtectedPeriods = [new ProtectedPeriodDTO
                {
                    StartMonth = 12, StartDay = 1, EndMonth = 1, EndDay = 31
                }]
            };

            var result = await _service.AddRegulation(add);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateRegulation_ShouldThrow_WhenPeriodNamesAnImpossibleDay()
        {
            _regulationsRepoMock
                .Setup(r => r.GetById(5, It.IsAny<System.Linq.Expressions.Expression<Func<SpeciesRegulation, object>>[]>(), false))
                .ReturnsAsync(new SpeciesRegulation { Id = 5, SpeciesId = 1 });
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(true);

            var update = new SpeciesRegulationUpdate
            {
                Id = 5,
                SpeciesId = 1,
                RegionId = 1,
                ProtectedPeriods = [new ProtectedPeriodDTO
                {
                    StartMonth = 4, StartDay = 31, EndMonth = 5, EndDay = 1
                }]
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateRegulation(5, update));
            Assert.Contains("not a valid day of month", ex.Message);
        }

        [Fact]
        public async Task GetRegulationsForSpecies_ShouldThrow_WhenSpeciesMissing()
        {
            _speciesRepoMock.Setup(s => s.Any(It.IsAny<Expression<Func<Species, bool>>>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetRegulationsForSpecies(99));
        }

        [Fact]
        public async Task GetRegulationsForSpecies_ShouldReturnNamesAndOrderNationalFirst()
        {
            _speciesRepoMock.Setup(s => s.Any(It.IsAny<Expression<Func<Species, bool>>>())).ReturnsAsync(true);

            var locationRule = new SpeciesRegulation
            {
                Id = 3,
                SpeciesId = 100,
                Locations = new List<Location> { new() { Id = 7, Name = "Kalajärvi" } },
                BagLimit = 4,
                BagLimitBasis = BagLimitBasis.Permit
            };
            var elyRule = new SpeciesRegulation
            {
                Id = 2,
                SpeciesId = 100,
                RegionId = 20,
                Region = new Region { Id = 20, Name = "Uusimaa ELY", Type = RegionType.Ely }
            };
            var nationalRule = new SpeciesRegulation
            {
                Id = 1,
                SpeciesId = 100,
                RegionId = 1,
                Region = new Region { Id = 1, Name = "Finland", Type = RegionType.National }
            };

            _regulationsRepoMock.Setup(r => r.GetForSpecies(100))
                .ReturnsAsync(new List<SpeciesRegulation> { locationRule, elyRule, nationalRule });

            var result = (await _service.GetRegulationsForSpecies(100)).ToList();

            Assert.Equal(3, result.Count);

            // National, then the rest of the tree, then location-scoped rules.
            Assert.Equal("Finland", result[0].Region!.Name);
            Assert.Equal(RegionType.National, result[0].Region!.Type);
            Assert.Equal("Uusimaa ELY", result[1].Region!.Name);

            // The whole point of this endpoint: names, not just ids.
            Assert.Null(result[2].Region);
            var water = Assert.Single(result[2].Locations);
            Assert.Equal(7, water.Id);
            Assert.Equal("Kalajärvi", water.Name);
            Assert.Equal(BagLimitBasis.Permit, result[2].BagLimitBasis);
        }
    }
}
