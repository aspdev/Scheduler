using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scheduler.Api.Extensions;


namespace Scheduler.Api
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;

        public Startup(IHostingEnvironment env, ILogger<Startup> logger)
        {
            _logger = logger;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "https://identityserver.arantasar.hostingasp.pl";
                    options.RequireHttpsMetadata = true;
                    options.ApiName = "api1";

                });
            
            services.AddCors(options =>
            {
                options.AddPolicy("SchedulerApiPolicy", policy =>
                    policy.WithOrigins("https://smartscheduler.pl")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMvc();
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                options.HttpsPort = 443; 
               
            });
            
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseCors("SchedulerApiPolicy");
            app.ConfigureExceptionMiddleware(_logger, env);
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
