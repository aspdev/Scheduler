using System.Threading.Tasks;
using Common;

namespace IdentityServer
{
    public interface IUserRepository
    {
        Task<bool> ValidateCredentials(string username, string password, string clientName);

        Task<User> FindBySubjectId(string subjectId);

        Task<User> FindByUsernameAndClientName(string username, string clientName);

        Task<User> FindByUsername(string username);

        Task UpdateUser(User user);
    }
}
