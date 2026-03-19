using Mapster;
using FishingMap.Data.Entities;
using FishingMap.Common.Extensions;
using FishingMap.Domain.DTO.Geometries;
using FishingMap.Domain.DTO.Images;
using FishingMap.Domain.DTO.Locations;
using FishingMap.Domain.DTO.Permits;
using FishingMap.Domain.DTO.Species;
using FishingMap.Domain.DTO.Users;

namespace FishingMap.Domain.MapsterConfig
{
    public class MapsterRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<NetTopologySuite.Geometries.Point, GeoPoint>()
                .Map(dest => dest.Latitude, src => src.Y)
                .Map(dest => dest.Longitude, src => src.X);

            config.NewConfig<Location, LocationDTO>()
                .Map(dest => dest.Geometry, src => src.Geometry != null ? src.Geometry.ToGeoJsonFeature() : null);
        }
    }
}