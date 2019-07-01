using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace Client.Torun.RavenDataService.Extentions
{
    public static class HttpRequestExtention
    {
        public static bool ContainsPathBase(this HttpRequest httpRequest, string pathBaseName)
        {
            var pathBase = httpRequest.Path.Value.Split('/', StringSplitOptions.RemoveEmptyEntries).First();

            return pathBase.Equals(pathBaseName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
