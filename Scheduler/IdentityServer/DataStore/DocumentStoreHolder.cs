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

        public DocumentStoreHolder(IConfiguration configuration, IHostingEnvironment environment)
        {
            var certPassword = configuration.GetSection("Certificates")["Raven.Certificate.Password"];
            /*var certPath = configuration.GetSection("Certificates")["Raven.Certificate.Path"];*/
            var certPath = environment.ContentRootPath +
                           "\\Certificates\\free.scheduler.client.certificate.with.password.pfx"; 
            
            var secureString = new SecureString();
            certPassword.ToCharArray().ToList().ForEach(secureString.AppendChar);
            secureString.MakeReadOnly();

            var bytes = File.ReadAllBytes(certPath);
            
            var store = new DocumentStore
            {
                Urls = new [] {"https://a.free.scheduler.ravendb.cloud"},
                Database = "IdentityServerUsers",
                Certificate = new X509Certificate2(bytes, certPassword, X509KeyStorageFlags.MachineKeySet)
            };

            Store = store.Initialize();
            
        }
       
        

        
    }
}
