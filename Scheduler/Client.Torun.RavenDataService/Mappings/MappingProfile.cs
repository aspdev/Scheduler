using AutoMapper;
using Client.Torun.RavenDataService.Entities;
using Client.Torun.RavenDataService.Helpers;
using Client.Torun.RavenDataService.Models;
using Client.Torun.Shared.DTOs;

namespace Client.Torun.RavenDataService.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserToCreateDto, User>()
                .AfterMap((s, d) => {
                    d.Id = string.Empty;
                    d.Clients.Add("scheduler-client-torun");
                });
            CreateMap<User, PostCreationUserToReturnDto>();
            CreateMap<User, UserToReturnDto>();
            CreateMap<User, DoctorDto>()
                .ForMember(d => d.DoctorId, opt => opt.MapFrom(src => src.Id))
                .ForMember(d => d.Name, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(d => d.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(d => d.Roles, opt => opt.MapFrom(src => src.Roles))
                .ForMember(d => d.Color, opt => opt.MapFrom(src => src.Color));
            CreateMap<RequirementToSetDto, DutyRequirement>()
                .AfterMap((s, d) => { d.Id = string.Empty; });
            CreateMap<DutyRequirement, PostCreationRequirementToReturnDto>();
            CreateMap<DayOffToSetDto, DayOff>();
            CreateMap<DayOff, DayOffToReturnDto>()
                .ForMember(d => d.Date, opt => opt.MapFrom(src => src.Date.Date.ToString("yyyy-MM-dd")));

        }
    }
}
