using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Scheduler.Shared.CustomExceptions;

namespace Scheduler.Api.Extensions
{
    public static class ExceptionMiddlewareExtention
    {
        public static void ConfigureExceptionMiddleware(this IApplicationBuilder app, ILogger logger, IHostingEnvironment environment)
        {

            //if (environment.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
                app.UseExceptionHandler(appError =>
                {
                    appError.Run(async context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        context.Response.ContentType = "application/json";

                        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

                        if (contextFeature != null)
                        {
                            var ex = contextFeature.Error;
                            string message = null;

                            if (ex.GetType() == typeof(AggregateException))
                            {
                                var exception = (AggregateException)ex;

                                exception.Flatten().Handle(innerException =>
                                {
                                    var apiException = (ApiException)innerException;
                                    message = apiException.Content;
                                    logger.LogError(message);
                                    return true;
                                });

                                await context.Response.WriteAsync("An unexpected fault happened. Try again later.");

                                return;
                            }


                            message = ex.ToString();
                            logger.LogError(message);

                            await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                        }


                    });


                });
            //}

            
        }
    }
}
