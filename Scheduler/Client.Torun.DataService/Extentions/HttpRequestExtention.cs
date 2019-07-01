using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Client.Torun.DataService.Extentions
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
