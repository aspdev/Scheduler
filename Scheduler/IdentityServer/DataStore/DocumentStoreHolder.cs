using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;

namespace IdentityServer.DataStore
{
    public class DocumentStoreHolder : IDocumentStoreHolder
    {
        public IDocumentStore Store { get; }

        public DocumentStoreHolder(IdentityServerConfiguration configuration, IHostingEnvironment environment)
        {
            var certPassword = configuration.Certificates.RavenCertificatePassword;
            var certPath = environment.ContentRootPath + configuration.Certificates.RavenCertificatePath;
            
            var secureString = new SecureString();
            certPassword.ToCharArray().ToList().ForEach(secureString.AppendChar);
            secureString.MakeReadOnly();

            var bytes = File.ReadAllBytes(certPath);
            
            var store = new DocumentStore
            {
                Urls = new [] { configuration.DatabaseUrl },
                Database = configuration.Database,
                Certificate = new X509Certificate2(bytes, certPassword, X509KeyStorageFlags.MachineKeySet)
            };

            Store = store.Initialize();
        }
    }
}
