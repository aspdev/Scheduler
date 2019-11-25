// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer
{
    public static class Config
    {
         // mapowane są na tzw. SCOPES, które dają dostęp do informacji o użytkownikach (Claims)
        // identity-related resources SCOPES

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(), // zapewnia, że User subjectId zostaje załączony
                // jeśli aplikacja kliencka zarząda openId SCOPE zwracany jest user identifier claim (subject)
                new IdentityResources.Profile(),
                // mapuje do profile related clains (name, website)
                // jeżeli zarządamy Profile SCOPE (name, website)claims zostaną zwrócone
                new IdentityResources.Address()
            };
        }

        // mapowane są na SCOPES, które dają dostęp do infromacji o zasobach (APIs)
        // api-related resources

        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource("api1", "Scheduler.Api"),
                new ApiResource("api2", "Client.Torun.RavenDataService")
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "scheduler-client-torun",
                    ClientName = "Scheduler",
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,
                    
                    // the URL of the CLIENT where you get the results
                    RedirectUris =           { "http://localhost:8081/callback/" }, 
                    PostLogoutRedirectUris = { "http://localhost:8081/" },
                    AllowedCorsOrigins =     { "http://localhost:8081/" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId, // user's unique ID (subject claim in .NET/ subject Identifier)
                        IdentityServerConstants.StandardScopes.Profile, // name, family_name, middle_name, profile, picture, gender, birthdate
                        "api1",
                        "api2",
                        IdentityServerConstants.StandardScopes.Address
                    },
                    
                    AllowOfflineAccess = true
                }
            };
        }
    }
}