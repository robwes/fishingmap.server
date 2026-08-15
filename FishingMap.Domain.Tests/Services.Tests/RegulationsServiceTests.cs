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
        private readonly Mock<ILocationSpeciesFollowsRegionRepository> _followsRegionMock;
        private readonly IMapper _mapper;
        private readonly RegulationsService _service;

        public RegulationsServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _regionsRepoMock = new Mock<IRegionRepository>();
            _regulationsRepoMock = new Mock<ISpeciesRegulationRepository>();
            _speciesRepoMock = new Mock<ISpeciesRepository>();
            _locationsRepoMock = new Mock<ILocationRepository>();
            _followsRegionMock = new Mock<ILocationSpeciesFollowsRegionRepository>();

            // Inheritance is opt-in, so a region rule only reaches a water for species it
            // follows. Species 100 is what every cascade test below rules on; the gate
            // itself is covered by the opt-in tests at the end of this file.
            _followsRegionMock.Setup(f => f.GetFollowedSpeciesIds(It.IsAny<int>()))
                .ReturnsAsync(new List<int> { 100 });
            _followsRegionMock.Setup(f => f.GetAll(
                It.IsAny<Expression<Func<LocationSpeciesFollowsRegion, bool>>>(),
                It.IsAny<Expression<Func<LocationSpeciesFollowsRegion, object>>[]?>(),
                It.IsAny<Func<IQueryable<LocationSpeciesFollowsRegion>, IOrderedQueryable<LocationSpeciesFollowsRegion>>?>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new List<LocationSpeciesFollowsRegion>());

            _unitOfWorkMock.Setup(u => u.LocationSpeciesFollowsRegions).Returns(_followsRegionMock.Object);
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
                new() { Id = 1, Name = "Finland", Type = RegionType.Root }
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
                new() { Id = 1, Name = "Finland", Type = RegionType.Root }
            });

            var nationalRule = new SpeciesRegulation
            {
                Id = 1,
                SpeciesId = 100,
                RegionId = 1,
                Region = new Region { Id = 1, Type = RegionType.Root, Name = "Finland" },
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
            _regionsRepoMock.Setup(r => r.GetRootRegionId()).ReturnsAsync(1);

            var nationalRule = new SpeciesRegulation
            {
                Id = 1,
                SpeciesId = 100,
                RegionId = 1,
                Region = new Region { Id = 1, Type = RegionType.Root, Name = "Finland" },
                MinimumSizeCm = 20
            };

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation> { nationalRule });

            var rules = (await _service.GetEffectiveRulesForLocation(5)).ToList();

            Assert.Single(rules);
            Assert.Equal("National", rules[0].Source);
            _regionsRepoMock.Verify(r => r.GetRootRegionId(), Times.Once);
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
            _regionsRepoMock.Setup(r => r.GetRootRegionId()).ReturnsAsync(1);

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
                new() { Id = 1, Name = "Finland", Type = RegionType.Root }
            });

            var nationalRule = new SpeciesRegulation
            {
                Id = 1,
                SpeciesId = 100,
                RegionId = 1,
                Region = new Region { Id = 1, Type = RegionType.Root, Name = "Finland" },
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
            _regionsRepoMock.Setup(r => r.GetRootRegionId()).ReturnsAsync(1);

            var only = new SpeciesRegulation
            {
                Id = 1,
                SpeciesId = 100,
                RegionId = 1,
                Region = new Region { Id = 1, Type = RegionType.Root, Name = "Finland" }
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
                Region = new Region { Id = 1, Name = "Finland", Type = RegionType.Root }
            };

            _regulationsRepoMock.Setup(r => r.GetForSpecies(100))
                .ReturnsAsync(new List<SpeciesRegulation> { locationRule, elyRule, nationalRule });

            var result = (await _service.GetRegulationsForSpecies(100)).ToList();

            Assert.Equal(3, result.Count);

            // National, then the rest of the tree, then location-scoped rules.
            Assert.Equal("Finland", result[0].Region!.Name);
            Assert.Equal(RegionType.Root, result[0].Region!.Type);
            Assert.Equal("Uusimaa ELY", result[1].Region!.Name);

            // The whole point of this endpoint: names, not just ids.
            Assert.Null(result[2].Region);
            var water = Assert.Single(result[2].Locations);
            Assert.Equal(7, water.Id);
            Assert.Equal("Kalajärvi", water.Name);
            Assert.Equal(BagLimitBasis.Permit, result[2].BagLimitBasis);
        }

        /// <summary>
        /// Puts a water under Sub -> Ely -> Finland, the shape the variant tests below share.
        /// </summary>
        private Location GivenLocationInFullChain()
        {
            var location = new Location { Id = 5, RegionId = 10 };
            _locationsRepoMock.Setup(l => l.GetById(5, null, true)).ReturnsAsync(location);
            _regionsRepoMock.Setup(r => r.GetAncestry(10)).ReturnsAsync(new List<Region>
            {
                new() { Id = 10, Name = "Sub", Type = RegionType.ManagementArea, ParentRegionId = 20 },
                new() { Id = 20, Name = "Ely", Type = RegionType.Ely, ParentRegionId = 1 },
                new() { Id = 1, Name = "Finland", Type = RegionType.Root }
            });
            return location;
        }

        /// <summary>
        /// A region-scoped trout rule.
        /// </summary>
        /// <param name="id">Regulation id.</param>
        /// <param name="regionId">Region it is scoped to; 1 = Finland, 20 = Ely.</param>
        /// <param name="fin">Fin state it is narrowed to, or null for all trout.</param>
        /// <param name="minimumSizeCm">Minimum size, used to tell the rules apart.</param>
        private static SpeciesRegulation TroutRule(int id, int regionId, AdiposeFin? fin, decimal? minimumSizeCm)
        {
            var type = regionId == 1 ? RegionType.Root : RegionType.Ely;
            return new SpeciesRegulation
            {
                Id = id,
                SpeciesId = 100,
                RegionId = regionId,
                Region = new Region { Id = regionId, Type = type, Name = regionId == 1 ? "Finland" : "Ely" },
                AdiposeFin = fin,
                MinimumSizeCm = minimumSizeCm
            };
        }

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldSplitASpeciesIntoOneRulePerFinState()
        {
            GivenLocationInFullChain();
            var intact = TroutRule(1, 1, AdiposeFin.Intact, null);
            intact.IsFullyProtected = true;
            var clipped = TroutRule(2, 1, AdiposeFin.Clipped, 50);

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation> { intact, clipped });

            var rules = (await _service.GetEffectiveRulesForLocation(5)).ToList();

            Assert.Equal(2, rules.Count);
            var intactRule = Assert.Single(rules, r => r.AdiposeFin == AdiposeFin.Intact);
            Assert.True(intactRule.IsFullyProtected);
            var clippedRule = Assert.Single(rules, r => r.AdiposeFin == AdiposeFin.Clipped);
            Assert.Equal(50, clippedRule.MinimumSizeCm);
        }

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldReturnOneUnlabelledRule_WhenNoRuleNamesAFinState()
        {
            // The behaviour every species had before variants existed, and still has.
            GivenLocationInFullChain();

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation> { TroutRule(1, 1, null, 40) });

            var rule = Assert.Single(await _service.GetEffectiveRulesForLocation(5));

            Assert.Null(rule.AdiposeFin);
            Assert.Equal(40, rule.MinimumSizeCm);
        }

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldCoverTheOtherFinState_WithTheUnqualifiedRule()
        {
            // An intact-only rule alongside a rule for all trout must not drop clipped fish:
            // the unqualified rule is what applies to them, labelled as such.
            GivenLocationInFullChain();
            var intact = TroutRule(1, 1, AdiposeFin.Intact, null);
            intact.IsFullyProtected = true;

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation> { intact, TroutRule(2, 1, null, 60) });

            var rules = (await _service.GetEffectiveRulesForLocation(5)).ToList();

            Assert.Equal(2, rules.Count);
            Assert.True(Assert.Single(rules, r => r.AdiposeFin == AdiposeFin.Intact).IsFullyProtected);
            Assert.Equal(60, Assert.Single(rules, r => r.AdiposeFin == AdiposeFin.Clipped).MinimumSizeCm);
        }

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldLetANearerRegionBeatAnExactFinMatch()
        {
            // Region specificity is the primary axis. An ELY rule for all trout overrides a
            // national intact-only rule, because the nearer authority spoke about this water.
            GivenLocationInFullChain();

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation>
                {
                    TroutRule(1, 1, AdiposeFin.Intact, 45),
                    TroutRule(2, 20, null, 55)
                });

            var rules = (await _service.GetEffectiveRulesForLocation(5)).ToList();

            // One winner for both fin states, so they collapse into a single unlabelled rule.
            var rule = Assert.Single(rules);
            Assert.Null(rule.AdiposeFin);
            Assert.Equal(55, rule.MinimumSizeCm);
        }

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldPreferTheExactFinMatch_AtEqualRegionRank()
        {
            GivenLocationInFullChain();

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation>
                {
                    TroutRule(1, 1, null, 60),
                    TroutRule(2, 1, AdiposeFin.Clipped, 50)
                });

            var rules = (await _service.GetEffectiveRulesForLocation(5)).ToList();

            var clipped = Assert.Single(rules, r => r.AdiposeFin == AdiposeFin.Clipped);
            Assert.Equal(50, clipped.MinimumSizeCm);
            // ...and the unqualified rule is what it shadows, so the editor can say so.
            Assert.Equal(60, clipped.FallsBackTo!.MinimumSizeCm);
        }

        [Fact]
        public async Task AddRegulation_ShouldAllowASecondVariantAtTheSameWater()
        {
            _speciesRepoMock.Setup(s => s.Any(It.IsAny<Expression<Func<Species, bool>>>())).ReturnsAsync(true);
            _locationsRepoMock.Setup(l => l.GetAll(
                It.IsAny<Expression<Func<Location, bool>>>(), null, null, false))
                .ReturnsAsync(new List<Location> { new() { Id = 5 } });

            // The duplicate check runs against the fin state too, so the existing
            // intact-finned rule at this water does not block the clipped one.
            Expression<Func<SpeciesRegulation, bool>>? predicate = null;
            _regulationsRepoMock.Setup(r => r.Find(
                It.IsAny<Expression<Func<SpeciesRegulation, bool>>>(),
                It.IsAny<Expression<Func<SpeciesRegulation, object>>[]?>(),
                It.IsAny<bool>()))
                .Callback<Expression<Func<SpeciesRegulation, bool>>, Expression<Func<SpeciesRegulation, object>>[]?, bool>(
                    (p, _, _) => predicate = p)
                .ReturnsAsync((SpeciesRegulation?)null);
            _regulationsRepoMock.Setup(r => r.Add(It.IsAny<SpeciesRegulation>()))
                .Returns<SpeciesRegulation>(r => { r.Id = 9; return r; });

            await _service.AddRegulation(new SpeciesRegulationAdd
            {
                SpeciesId = 100,
                LocationIds = new[] { 5 },
                AdiposeFin = AdiposeFin.Clipped,
                MinimumSizeCm = 50
            });

            var existingIntactRule = new SpeciesRegulation
            {
                Id = 1,
                SpeciesId = 100,
                AdiposeFin = AdiposeFin.Intact,
                Locations = new List<Location> { new() { Id = 5 } }
            };
            Assert.False(predicate!.Compile()(existingIntactRule));
        }

        [Fact]
        public async Task AddRegulation_ShouldPersistTheFinStateAndFullProtection()
        {
            _regionsRepoMock.Setup(r => r.Any(It.IsAny<Expression<Func<Region, bool>>>())).ReturnsAsync(true);
            _speciesRepoMock.Setup(s => s.Any(It.IsAny<Expression<Func<Species, bool>>>())).ReturnsAsync(true);

            SpeciesRegulation? captured = null;
            _regulationsRepoMock.Setup(r => r.Add(It.IsAny<SpeciesRegulation>()))
                .Returns<SpeciesRegulation>(r => { r.Id = 7; captured = r; return r; });

            await _service.AddRegulation(new SpeciesRegulationAdd
            {
                SpeciesId = 100,
                RegionId = 1,
                AdiposeFin = AdiposeFin.Intact,
                IsFullyProtected = true
            });

            Assert.Equal(AdiposeFin.Intact, captured!.AdiposeFin);
            Assert.True(captured.IsFullyProtected);
            // Full protection is not catch-and-release; entering one must not set the other.
            Assert.False(captured.IsCatchAndReleaseOnly);
        }

        // Decision 11: inheritance is opt-in. A region rule reaches a water only for species
        // an administrator chose to follow.

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldNotInheritForASpeciesNobodyChose()
        {
            GivenLocationInFullChain();
            _followsRegionMock.Setup(f => f.GetFollowedSpeciesIds(5)).ReturnsAsync(new List<int>());

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation> { TroutRule(1, 1, null, 40) });

            var rules = await _service.GetEffectiveRulesForLocation(5);

            // Not a rule with no restrictions — no rule at all. The water has nothing
            // recorded for this species, which the client must not render as "unrestricted".
            Assert.Empty(rules);
        }

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldInheritOnlyForFollowedSpecies()
        {
            GivenLocationInFullChain();
            _followsRegionMock.Setup(f => f.GetFollowedSpeciesIds(5)).ReturnsAsync(new List<int> { 100 });

            var otherSpecies = TroutRule(2, 1, null, 25);
            otherSpecies.SpeciesId = 200;

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation> { TroutRule(1, 1, null, 40), otherSpecies });

            var rules = (await _service.GetEffectiveRulesForLocation(5)).ToList();

            var rule = Assert.Single(rules);
            Assert.Equal(100, rule.SpeciesId);
        }

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldApplyALocalRuleWithoutFollowing()
        {
            // Writing a rule for a water IS the decision, so a location-scoped rule is never
            // gated — otherwise an override would vanish unless the species also followed.
            var location = new Location { Id = 5, RegionId = 10 };
            _locationsRepoMock.Setup(l => l.GetById(5, null, true)).ReturnsAsync(location);
            _regionsRepoMock.Setup(r => r.GetAncestry(10)).ReturnsAsync(new List<Region>
            {
                new() { Id = 10, Name = "Sub", Type = RegionType.ManagementArea, ParentRegionId = 1 },
                new() { Id = 1, Name = "Finland", Type = RegionType.Root }
            });
            _followsRegionMock.Setup(f => f.GetFollowedSpeciesIds(5)).ReturnsAsync(new List<int>());

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation>
                {
                    new()
                    {
                        Id = 1,
                        SpeciesId = 100,
                        Locations = new List<Location> { location },
                        MinimumSizeCm = 50
                    }
                });

            var rule = Assert.Single(await _service.GetEffectiveRulesForLocation(5));

            Assert.Equal(50, rule.MinimumSizeCm);
            Assert.Equal("Location", rule.Source);
        }

        [Fact]
        public async Task GetEffectiveRulesForLocation_ShouldNotFallBackToARuleTheWaterDoesNotFollow()
        {
            // The runner-up drives "revert to the inherited rule". Offering a fallback the
            // water never opted into would promise something reverting wouldn't deliver.
            var location = new Location { Id = 5, RegionId = 10 };
            _locationsRepoMock.Setup(l => l.GetById(5, null, true)).ReturnsAsync(location);
            _regionsRepoMock.Setup(r => r.GetAncestry(10)).ReturnsAsync(new List<Region>
            {
                new() { Id = 10, Name = "Sub", Type = RegionType.ManagementArea, ParentRegionId = 1 },
                new() { Id = 1, Name = "Finland", Type = RegionType.Root }
            });
            _followsRegionMock.Setup(f => f.GetFollowedSpeciesIds(5)).ReturnsAsync(new List<int>());

            _regulationsRepoMock.Setup(r => r.GetCandidatesForLocation(5, It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<SpeciesRegulation>
                {
                    new()
                    {
                        Id = 1,
                        SpeciesId = 100,
                        Locations = new List<Location> { location },
                        MinimumSizeCm = 50
                    },
                    TroutRule(2, 1, null, 40)
                });

            var rule = Assert.Single(await _service.GetEffectiveRulesForLocation(5));

            Assert.Equal(50, rule.MinimumSizeCm);
            Assert.Null(rule.FallsBackTo);
        }

        /// <summary>
        /// Sets up SetFollowsRegion's existence checks to pass.
        /// </summary>
        private void GivenLocationAndSpeciesExist()
        {
            _locationsRepoMock.Setup(l => l.Any(It.IsAny<Expression<Func<Location, bool>>>())).ReturnsAsync(true);
            _speciesRepoMock.Setup(s => s.Any(It.IsAny<Expression<Func<Species, bool>>>())).ReturnsAsync(true);
        }

        /// <summary>
        /// Makes the follow lookup return the given row, or none.
        /// </summary>
        private void GivenExistingFollowRow(LocationSpeciesFollowsRegion? row)
        {
            _followsRegionMock.Setup(f => f.Find(
                It.IsAny<Expression<Func<LocationSpeciesFollowsRegion, bool>>>(),
                It.IsAny<Expression<Func<LocationSpeciesFollowsRegion, object>>[]?>(),
                It.IsAny<bool>()))
                .ReturnsAsync(row);
        }

        [Fact]
        public async Task SetFollowsRegion_ShouldRecordTheDecision()
        {
            GivenLocationAndSpeciesExist();
            GivenExistingFollowRow(null);
            _regulationsRepoMock.Setup(r => r.Find(
                It.IsAny<Expression<Func<SpeciesRegulation, bool>>>(),
                It.IsAny<Expression<Func<SpeciesRegulation, object>>[]?>(),
                It.IsAny<bool>()))
                .ReturnsAsync((SpeciesRegulation?)null);

            LocationSpeciesFollowsRegion? added = null;
            _followsRegionMock.Setup(f => f.Add(It.IsAny<LocationSpeciesFollowsRegion>()))
                .Returns<LocationSpeciesFollowsRegion>(f => { added = f; return f; });

            await _service.SetFollowsRegion(5, 100, follows: true);

            Assert.Equal(5, added!.LocationId);
            Assert.Equal(100, added.SpeciesId);
        }

        [Fact]
        public async Task SetFollowsRegion_ShouldRefuseWhileTheWaterHasItsOwnRule()
        {
            // Following and overriding are exclusive, and silently deleting an authored rule
            // on a toggle would lose work. The caller removes it first.
            GivenLocationAndSpeciesExist();
            GivenExistingFollowRow(null);
            _regulationsRepoMock.Setup(r => r.Find(
                It.IsAny<Expression<Func<SpeciesRegulation, bool>>>(),
                It.IsAny<Expression<Func<SpeciesRegulation, object>>[]?>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new SpeciesRegulation { Id = 9, SpeciesId = 100 });

            var ex = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.SetFollowsRegion(5, 100, follows: true));

            Assert.Contains("Remove it before following", ex.Message);
            _followsRegionMock.Verify(f => f.Add(It.IsAny<LocationSpeciesFollowsRegion>()), Times.Never);
        }

        [Fact]
        public async Task SetFollowsRegion_ShouldBeANoOp_WhenAlreadyFollowing()
        {
            GivenLocationAndSpeciesExist();
            GivenExistingFollowRow(new LocationSpeciesFollowsRegion { Id = 3, LocationId = 5, SpeciesId = 100 });

            await _service.SetFollowsRegion(5, 100, follows: true);

            _followsRegionMock.Verify(f => f.Add(It.IsAny<LocationSpeciesFollowsRegion>()), Times.Never);
        }

        [Fact]
        public async Task SetFollowsRegion_ShouldRemoveTheDecision_WhenTurnedOff()
        {
            var row = new LocationSpeciesFollowsRegion { Id = 3, LocationId = 5, SpeciesId = 100 };
            GivenLocationAndSpeciesExist();
            GivenExistingFollowRow(row);

            await _service.SetFollowsRegion(5, 100, follows: false);

            _followsRegionMock.Verify(f => f.Delete(row), Times.Once);
        }

        [Fact]
        public async Task SetFollowsRegion_ShouldThrow_WhenTheLocationOrSpeciesIsMissing()
        {
            _locationsRepoMock.Setup(l => l.Any(It.IsAny<Expression<Func<Location, bool>>>())).ReturnsAsync(false);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.SetFollowsRegion(99, 100, true));
        }

        [Fact]
        public async Task AddRegulation_ShouldStopTheWaterFollowingTheRegionForThatSpecies()
        {
            // Writing a custom rule is the decision to stop inheriting there. Leaving the
            // row would make the species both follow and override at once.
            _speciesRepoMock.Setup(s => s.Any(It.IsAny<Expression<Func<Species, bool>>>())).ReturnsAsync(true);
            _locationsRepoMock.Setup(l => l.GetAll(
                It.IsAny<Expression<Func<Location, bool>>>(), null, null, false))
                .ReturnsAsync(new List<Location> { new() { Id = 5 } });
            _regulationsRepoMock.Setup(r => r.Find(
                It.IsAny<Expression<Func<SpeciesRegulation, bool>>>(),
                It.IsAny<Expression<Func<SpeciesRegulation, object>>[]?>(),
                It.IsAny<bool>()))
                .ReturnsAsync((SpeciesRegulation?)null);
            _regulationsRepoMock.Setup(r => r.Add(It.IsAny<SpeciesRegulation>()))
                .Returns<SpeciesRegulation>(r => { r.Id = 9; return r; });

            var followRow = new LocationSpeciesFollowsRegion { Id = 3, LocationId = 5, SpeciesId = 100 };
            _followsRegionMock.Setup(f => f.GetAll(
                It.IsAny<Expression<Func<LocationSpeciesFollowsRegion, bool>>>(),
                It.IsAny<Expression<Func<LocationSpeciesFollowsRegion, object>>[]?>(),
                It.IsAny<Func<IQueryable<LocationSpeciesFollowsRegion>, IOrderedQueryable<LocationSpeciesFollowsRegion>>?>(),
                It.IsAny<bool>()))
                .ReturnsAsync(new List<LocationSpeciesFollowsRegion> { followRow });

            await _service.AddRegulation(new SpeciesRegulationAdd
            {
                SpeciesId = 100,
                LocationIds = new[] { 5 }
            });

            _followsRegionMock.Verify(f => f.Delete(followRow), Times.Once);
        }
    }
}
