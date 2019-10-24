using AutoMapper;
using Client.Torun.RavenDataService.Config;
using Client.Torun.RavenDataService.Entities;
using Client.Torun.RavenDataService.Helpers;
using Client.Torun.RavenDataService.Models;
using Client.Torun.Shared.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Constraints;

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
                    d.TemporaryPassword = RandomPasswordGenerator.GeneratePassword(15);
                });
            CreateMap<User, PostCreationUserToReturnDto>();
            CreateMap<User, UserToReturnDto>();
            CreateMap<User, DoctorDto>()
                .ForMember(d => d.DoctorId, opt => opt.MapFrom(src => src.Id))
                .ForMember(d => d.Name, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(d => d.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(d => d.Roles, opt => opt.MapFrom(src => src.Roles));
            CreateMap<RequirementToSetDto, DutyRequirement>()
                .AfterMap((s, d) => { d.Id = string.Empty; });
            CreateMap<DutyRequirement, PostCreationRequirementToReturnDto>();

        }
    }
}
