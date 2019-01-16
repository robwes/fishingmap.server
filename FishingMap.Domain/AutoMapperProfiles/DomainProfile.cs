using AutoMapper;
using FishingMap.Domain.Data.DTO;
using FishingMap.Domain.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FishingMap.Domain.AutoMapperProfiles
{
    public class DomainProfile : Profile
    {
        public DomainProfile()
        {
            CreateMap<Data.Entities.Species, Species>();
            CreateMap<Data.Entities.LocationOwner, LocationOwner>();

            CreateMap<Data.Entities.Location, Location>()
                .ForMember(dest => dest.Points,
                    opts => opts.MapFrom(src => src.Points.ToGeoJsonFeature())
                )
                .ForMember(dest => dest.Position,
                    opts => opts.MapFrom(src => new GeoPoint() { Latitude = src.Position.Y, Longitude = src.Position.X })
                )
                .ForMember(dest => dest.Species,
                    opts => opts.MapFrom(src => src.LocationSpecies.Select(ls => new Species() { Id = ls.SpeciesId, Name = ls.Species.Name, Description = ls.Species.Description}))
                );

            CreateMap<Data.Entities.Location, LocationMarker>()
                .ForMember(dest => dest.Position,
                    opts => opts.MapFrom(src => new GeoPoint() { Latitude = src.Position.Y, Longitude = src.Position.X })
                );


        }
    }
}