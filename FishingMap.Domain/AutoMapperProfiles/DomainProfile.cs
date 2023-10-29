using AutoMapper;
using FishingMap.Domain.Data.DTO.GeoObjects;
using FishingMap.Domain.Data.DTO.ImageObjects;
using FishingMap.Domain.Data.DTO.LocationObjects;
using FishingMap.Domain.Data.DTO.PermitObjects;
using FishingMap.Domain.Data.DTO.SpeciesObjects;
using FishingMap.Domain.Data.DTO.UserObjects;
using FishingMap.Domain.Extensions;

namespace FishingMap.Domain.AutoMapperProfiles
{
    public class DomainProfile : Profile
    {
        public DomainProfile()
        {
            CreateMap<Data.Entities.Location, LocationSummary>();
            CreateMap<Data.Entities.Location, LocationMarker>();
            CreateMap<Data.Entities.LocationOwner, LocationOwner>();

            CreateMap<Data.Entities.Image, Image>();
            CreateMap<Data.Entities.Permit, Permit>();
            CreateMap<Data.Entities.Role, Role>();

            CreateMap<Data.Entities.Species, Species>();
            CreateMap<Data.Entities.Species, SpeciesIdName>();

            CreateMap<Data.Entities.User, User>();
            CreateMap<Data.Entities.User, UserCredentials>();
            
            CreateMap<NetTopologySuite.Geometries.Point, GeoPoint>()
                .ForMember(dest => dest.Latitude,
                    opts => opts.MapFrom(src => src.Y)
                )
                .ForMember(dest => dest.Longitude,
                    opts => opts.MapFrom(src => src.X)
                );

            CreateMap<Data.Entities.Location, Location>()
                .ForMember(dest => dest.Geometry,
                    opts => opts.MapFrom(src => src.Geometry.ToGeoJsonFeature())
                );
        }
    }
}