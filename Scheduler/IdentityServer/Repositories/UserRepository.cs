using IdentityServer.DataStore;
using IdentityServer.Entities;
using Raven.Client.Documents;
using System;
using System.Linq;
using System.Threading.Tasks;
using Common;

namespace IdentityServer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private IDocumentStore _store;

        public UserRepository(IDocumentStoreHolder storeHolder)
        {
            _store = storeHolder.Store;
        }
        public async Task<User> FindBySubjectId(string subjectId)
        {
            using (var session = _store.OpenAsyncSession())
            {
                var user = await session.Query<User>().FirstOrDefaultAsync(u => u.Id == subjectId);

                return user;
            }

               
        }

        public async Task<User> FindByUsername(string username, string clientName)
        {
            using (var session = _store.OpenAsyncSession())
            {
                var user = await session.Query<User>()
                    .FirstOrDefaultAsync(u => u.Email.Equals(username) && u.Clients.Contains(clientName));

                return user;
            }

            
        }

        public async Task<bool> ValidateCredentials(string username, string password, string clientName)
        {
            var user = await FindByUsername(username, clientName);

            var salt = user.Salt;
            var hashedPassword = PasswordHasher.HashPassword(password, Convert.FromBase64String(salt));
            
            if(user != null && user.ChangePassword == false)
            {
                return user.Password.Equals(hashedPassword);
            }
            else if (user != null && user.ChangePassword == true)
            {
                return user.TemporaryPassword.Equals(hashedPassword);
            }

            return false;
        }
    }
}
