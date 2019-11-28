using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;

namespace IdentityServer.DataStore
{
    public class DocumentStoreHolder : IDocumentStoreHolder
    {
        public IDocumentStore Store { get; }

        public DocumentStoreHolder(IConfiguration configuration)
        {
            var certPassword = configuration.GetSection("Certificates")["Raven.Certificate.Password"];
            var cerPath = configuration.GetSection("Certificates")["Raven.Certificate.Path"];
            
            var secureString = new SecureString();
            certPassword.ToCharArray().ToList().ForEach(secureString.AppendChar);
            secureString.MakeReadOnly();
            
            var store = new DocumentStore
            {
                Urls = new [] {"https://a.free.scheduler.ravendb.cloud"},
                Database = "IdentityServerUsers",
                Certificate = new X509Certificate2(cerPath, secureString)
            };

            Store = store.Initialize();
            
        }
       
        

        
    }
}
