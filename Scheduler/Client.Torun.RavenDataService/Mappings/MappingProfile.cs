﻿using AutoMapper;
using Client.Torun.RavenDataService.Config;
using Client.Torun.RavenDataService.Entities;
using Client.Torun.RavenDataService.Helpers;
using Client.Torun.RavenDataService.Models;
using Microsoft.AspNetCore.Http;

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
           
        }
    }
}
