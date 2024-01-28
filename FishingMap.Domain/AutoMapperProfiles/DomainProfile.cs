using AutoMapper;
using FishingMap.Data.Entities;
using FishingMap.Domain.Data.DTO.GeoObjects;
using FishingMap.Domain.Data.DTO.ImageObjects;
using FishingMap.Domain.Data.DTO.LocationObjects;
using FishingMap.Domain.Data.DTO.PermitObjects;
using FishingMap.Domain.Data.DTO.SpeciesObjects;
using FishingMap.Domain.Data.DTO.UserObjects;
using FishingMap.Common.Extensions;

namespace FishingMap.Domain.AutoMapperProfiles
{
    public class DomainProfile : Profile
    {
        public DomainProfile()
        {
            CreateMap<Location, LocationSummary>();
            CreateMap<Location, LocationMarker>();
            CreateMap<LocationOwner, LocationOwnerDTO>();

            CreateMap<Image, ImageDTO>();
            CreateMap<Permit, PermitDTO>();
            CreateMap<Role, RoleDTO>();

            CreateMap<Species, SpeciesDTO>();
            CreateMap<Species, SpeciesIdName>();

            CreateMap<User, UserDTO>();
            CreateMap<User, UserCredentials>();
            
            CreateMap<NetTopologySuite.Geometries.Point, GeoPoint>()
                .ForMember(dest => dest.Latitude,
                    opts => opts.MapFrom(src => src.Y)
                )
                .ForMember(dest => dest.Longitude,
                    opts => opts.MapFrom(src => src.X)
                );

            CreateMap<Location, LocationDTO>()
                .ForMember(dest => dest.Geometry,
                    opts => opts.MapFrom(src => src.Geometry.ToGeoJsonFeature())
                );
        }
    }
}