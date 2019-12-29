using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityServer.Extentions
{
    public static class IdentityServerBuilderExtentions
    {
        public static IIdentityServerBuilder LoadSigningCredentialsFrom(this IIdentityServerBuilder builder, 
            string path, string password)
        {
            var secureString = new SecureString();
            password.ToCharArray().ToList().ForEach(secureString.AppendChar);
            secureString.MakeReadOnly();
            
            var cert = new X509Certificate2(path, secureString, X509KeyStorageFlags.MachineKeySet);
            builder.AddSigningCredential(cert);
            return builder;
        }
    }
}