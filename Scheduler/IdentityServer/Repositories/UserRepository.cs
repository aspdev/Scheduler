using IdentityServer.DataStore;
using Raven.Client.Documents;
using System;
using System.Threading.Tasks;
using Common;

namespace IdentityServer.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDocumentStore _store;

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

        public async Task<User> FindByUsernameAndClientName(string username, string clientName)
        {
            using (var session = _store.OpenAsyncSession())
            {
                var user = await session.Query<User>()
                    .FirstOrDefaultAsync(u => u.Email.Equals(username) && u.Clients.Contains(clientName));

                return user;
            }
        }

        public async Task<User> FindByUsername(string username)
        {
            using (var session = _store.OpenAsyncSession())
            {
                var user = await session.Query<User>()
                    .FirstOrDefaultAsync(u => u.Email.Equals(username));

                return user;
            }
        }

        public async Task<bool> ValidateCredentials(string username, string password, string clientName)
        {
            var user = await FindByUsernameAndClientName(username, clientName);

            if (user is null)
            { 
                return false;
            }

            var salt = user.Salt;
            var hashedPassword = PasswordHasher.HashPassword(password, Convert.FromBase64String(salt));

            var isUserAuthenticated = user.ChangePassword == false
                ? user.Password.Equals(hashedPassword)
                : user.TemporaryPassword.Equals(hashedPassword);

            return isUserAuthenticated;
        }

        public async Task UpdateUser(User user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(User));
            }

            using (var session = _store.OpenAsyncSession())
            {
                await session.StoreAsync(user);
                await session.SaveChangesAsync();
            }
        }
    }
}
