using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Client.Torun.RavenDataService.Extentions
{
    public static class ExceptionMiddlewareExtention
    {
        public static void ConfigureExceptionMiddleware(this IApplicationBuilder app, ILogger logger, IHostingEnvironment environment)
        {

            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    var errorFeature = context.Features.Get<IExceptionHandlerFeature>();

                    if (errorFeature != null)
                    {
                        var exception = errorFeature.Error;


                        if (context.Request.ContainsPathBase("originator"))
                        {
                            await context.Response.WriteAsync(exception.ToString());

                            return;
                        }

                        var message = exception.ToString();
                        logger.LogError(message);

                        await context.Response.WriteAsync("An unexpected error occurred! Try again later");

                    }

                });
            });



        }
    }
}
