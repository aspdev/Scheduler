using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;

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
            builder.AddSigningCredential(new X509Certificate2(path, secureString));

            return builder;
        }
    }
}