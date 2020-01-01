using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using Client.Torun.RavenDataService.Config;
using Microsoft.AspNetCore.Hosting;
using Raven.Client.Documents;

namespace Client.Torun.RavenDataService.DataStore
{
    public class IdentityServerDocumentStoreHolder
    {
        public IDocumentStore Store { get; }

        public IdentityServerDocumentStoreHolder(DataServiceConfiguration configuration, IHostingEnvironment environment)
        {
            var certPassword = configuration.Certificates.RavenCertificatePassword;
            var certPath = environment.ContentRootPath +
                           "\\Certificates\\free.scheduler.client.certificate.with.password.pfx";
            
            var secureString = new SecureString();
            certPassword.ToCharArray().ToList().ForEach(secureString.AppendChar);
            secureString.MakeReadOnly();
            
            var store = new DocumentStore
            {
                Urls = configuration.Urls,
                Database = configuration.IdentityServerDatabase,
                Certificate = new X509Certificate2(certPath, secureString, X509KeyStorageFlags.MachineKeySet)
            };

            Store = store.Initialize();

        }
    }
}