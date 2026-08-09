using FishingMap.Data.Entities;
using FishingMap.Domain.DTO.Locations;
using FishingMap.Domain.MapsterConfig;
using Mapster;
using MapsterMapper;

namespace FishingMap.Domain.Tests.Mapping.Tests
{
    /// <summary>
    /// LocationSummary.RegionId is mapped by Mapster's name convention rather than
    /// by an explicit rule, so nothing in MapsterRegister points at it. The region
    /// admin screen filters waters on this field; without it the screen silently
    /// shows no waters for every region.
    /// </summary>
    public class LocationSummaryMappingTests
    {
        private readonly IMapper _mapper;

        public LocationSummaryMappingTests()
        {
            var config = new TypeAdapterConfig();
            config.Scan(typeof(MapsterRegister).Assembly);
            _mapper = new Mapper(config);
        }

        [Fact]
        public void MapsRegionIdOntoTheSummary()
        {
            var location = new Location { Id = 1, Name = "Kalajärvi", RegionId = 5 };

            var summary = _mapper.Map<LocationSummary>(location);

            Assert.Equal(5, summary.RegionId);
        }

        [Fact]
        public void LeavesRegionIdNullForAWaterOutsideEveryRegion()
        {
            var location = new Location { Id = 1, Name = "Nuuksio", RegionId = null };

            var summary = _mapper.Map<LocationSummary>(location);

            Assert.Null(summary.RegionId);
        }
    }
}
