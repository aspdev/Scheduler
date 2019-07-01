using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using Scheduler.Api.Extensions;


namespace Scheduler.Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            env.ConfigureNLog("Nlog.config");
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "https://localhost:44349";
                    options.RequireHttpsMetadata = true;
                    options.ApiName = "api1";

                });

            services.AddCors(options =>
            {
                options.AddPolicy("SchedulerApiPolicy", policy =>
                    policy.WithOrigins("http://localhost:8081")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMvc();
            
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            app.UseCors("SchedulerApiPolicy");

            var logger = loggerFactory.CreateLogger<Startup>();

            app.ConfigureExceptionMiddleware(logger, env);
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
