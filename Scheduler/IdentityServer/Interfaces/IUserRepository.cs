using IdentityServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer
{
    public interface IUserRepository
    {
        Task<bool> ValidateCredentials(string username, string password, string clientName);

        Task<User> FindBySubjectId(string subjectId);

        Task<User> FindByUsername(string username, string clientName);
    }
}
