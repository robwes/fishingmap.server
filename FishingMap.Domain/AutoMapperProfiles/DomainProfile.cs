﻿using AutoMapper;
using FishingMap.Domain.Data.DTO;
using FishingMap.Domain.Extensions;
using FishingMap.Domain.Interfaces;
using System.Linq;

namespace FishingMap.Domain.AutoMapperProfiles
{
    public class GetImagePath : IMappingAction<Data.Entities.Image, Image>
    {
        private readonly IFishingMapConfiguration _configuration;
        public GetImagePath(IFishingMapConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void Process(Data.Entities.Image source, Image destination, ResolutionContext context)
        {
            destination.Path = $"{_configuration.ImagesFolderPath}/{source.Path}";
        }
    }

    public class DomainProfile : Profile
    {
        public DomainProfile()
        {
            CreateMap<Data.Entities.Species, Species>();
            CreateMap<Data.Entities.LocationOwner, LocationOwner>();
            CreateMap<Data.Entities.User, User>();
            CreateMap<Data.Entities.User, UserCredentials>();
            CreateMap<Data.Entities.Role, Role>();
            CreateMap<Data.Entities.Image, Image>();
                //.AfterMap<GetImagePath>();

            CreateMap<Data.Entities.Location, Location>()
                .ForMember(dest => dest.Geometry,
                    opts => opts.MapFrom(src => src.Geometry.ToGeoJsonFeature())
                )
                .ForMember(dest => dest.Position,
                    opts => opts.MapFrom(src => new GeoPoint() { Latitude = src.Position.Y, Longitude = src.Position.X })
                )
                .ForMember(dest => dest.Species,
                    opts => opts.MapFrom(src => src.Species.Select(s => new Species() { Id = s.Id, Name = s.Name, Description = s.Description}))
                );

            CreateMap<Data.Entities.Location, LocationMarker>()
                .ForMember(dest => dest.Position,
                    opts => opts.MapFrom(src => new GeoPoint() { Latitude = src.Position.Y, Longitude = src.Position.X })
                );          
        }
    }
}