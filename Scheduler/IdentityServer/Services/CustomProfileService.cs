using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Services
{
    public class CustomProfileService : IProfileService
    {
        protected readonly ILogger Logger;

        protected readonly IUserRepository _userRepository;

        public CustomProfileService(IUserRepository userRepository, ILogger<CustomProfileService> logger)
        {
            _userRepository = userRepository;
            Logger = logger;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();

            Logger.LogDebug("Get profile called for subject {subject} from client {client} with claim types {claimTypes} via {caller}",
                context.Subject.GetSubjectId(), context.Client.ClientName ?? context.Client.ClientId,
                context.RequestedClaimTypes, context.Caller);

            var user = await _userRepository.FindBySubjectId(sub);

            var claims = new List<Claim>();

            if (user.Roles.Any())
            {
                List<string> roles = new List<string>();

                foreach(var role in user.Roles)
                {
                    roles.Add(role);
                    
                }

                var JsonRoles = JsonConvert.SerializeObject(roles);
                claims.Add(new Claim("roles", JsonRoles));
            }

            claims.Add(new Claim("firstName", user.FirstName));
            claims.Add(new Claim("lastName", user.LastName));
            claims.Add(new Claim("username", user.Email));
                                  
            context.IssuedClaims = claims;
                        
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub =  context.Subject.GetSubjectId();

            var user = await _userRepository.FindBySubjectId(sub);

            context.IsActive = user != null;

            
        }
    }
}
