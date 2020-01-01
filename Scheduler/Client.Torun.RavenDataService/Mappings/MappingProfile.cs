﻿using AutoMapper;
using Client.Torun.RavenDataService.Entities;
using Client.Torun.RavenDataService.Models;
using Client.Torun.Shared.DTOs;
using Common;

namespace Client.Torun.RavenDataService.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserToCreateDto, IdentityServerUser>()
                .AfterMap((s, d) => {
                    d.Id = string.Empty;
                    d.Clients.Add(s.Client);
                });
            CreateMap<IdentityServerUser, PostCreationUserToReturnDto>();
            CreateMap<IdentityServerUser, UserToReturnDto>();
            CreateMap<IdentityServerUser, DoctorDto>()
                .ForMember(d => d.DoctorId, opt => opt.MapFrom(src => src.Id))
                .ForMember(d => d.Name, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(d => d.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(d => d.Roles, opt => opt.MapFrom(src => src.Roles));
            CreateMap<RequirementToSetDto, DutyRequirement>()
                .AfterMap((s, d) => { d.Id = string.Empty; });
            CreateMap<DutyRequirement, PostCreationRequirementToReturnDto>();
            CreateMap<DayOffToSetDto, DayOff>();
            CreateMap<DayOff, DayOffToReturnDto>()
                .ForMember(d => d.Date, opt => opt.MapFrom(src => src.Date.Date.ToString("yyyy-MM-dd")));

        }
    }
}
