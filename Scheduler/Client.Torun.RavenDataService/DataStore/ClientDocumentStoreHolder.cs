using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using Client.Torun.RavenDataService.Config;
using Client.Torun.RavenDataService.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Internal;
using Raven.Client.Documents;

namespace Client.Torun.RavenDataService.DataStore
{
    public class ClientDocumentStoreHolder
    {
        public IDocumentStore Store { get; }

        public ClientDocumentStoreHolder(DataServiceConfiguration configuration, IHostingEnvironment environment)
        {
            var certPassword = configuration.Certificates.RavenCertificatePassword;
            var certPath = environment.ContentRootPath +
                          "\\Certificates\\free.scheduler.client.certificate.with.password.pfx";
            
            //var bytes = File.ReadAllBytes(certPath);
            
            var secureString = new SecureString();
            certPassword.ToCharArray().ToList().ForEach(secureString.AppendChar);
            secureString.MakeReadOnly();
            
            var store = new DocumentStore
            {
                Urls = configuration.Urls,
                Database = configuration.ClientDatabase,
                Certificate = new X509Certificate2(certPath, secureString, X509KeyStorageFlags.MachineKeySet)
            };

            Store = store.Initialize();
        }
    }
}
