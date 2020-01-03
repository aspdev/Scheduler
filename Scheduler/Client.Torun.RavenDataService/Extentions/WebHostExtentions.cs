using System.Threading.Tasks;
using Client.Torun.RavenDataService.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Client.Torun.RavenDataService.Extentions
{
    public static class WebHostExtentions
    {
        public static async Task InitAsync(this IWebHost webHost)
        {
            var asyncInitializers = webHost.Services.GetServices<IAsyncInitializer>();

            foreach (var initializer in asyncInitializers)
            {
               await initializer.InitializeAsync();
            }
        }
    }
}