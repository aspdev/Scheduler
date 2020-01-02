﻿using System.Threading.Tasks;
using Common;

namespace IdentityServer
{
    public interface IUserRepository
    {
        Task<bool> ValidateCredentials(string username, string password, string clientName);

        Task<IdentityServerUser> FindBySubjectId(string subjectId);

        Task<IdentityServerUser> FindByUsernameAndClientName(string username, string clientName);

        Task<IdentityServerUser> FindByUsername(string username);
    }
}
