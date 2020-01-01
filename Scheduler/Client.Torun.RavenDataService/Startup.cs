using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using Client.Torun.RavenDataService.Config;
using Client.Torun.RavenDataService.DataStore;
using Client.Torun.RavenDataService.Extentions;
using Client.Torun.RavenDataService.Mappings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scheduler.Mailer.Interfaces;
using Scheduler.Mailer.MailKit;

namespace Client.Torun.RavenDataService
{
    public class Startup
    {
        private readonly ILogger<Startup> _logger;

        private readonly DataServiceConfiguration _dataServiceConfiguration;
        private readonly IConfiguration _configuration;

        public Startup(IHostingEnvironment env, ILogger<Startup> logger)
        {
            _logger = logger;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                
            _configuration = builder.Build();

            _dataServiceConfiguration = new DataServiceConfiguration();
            _configuration.Bind("DataServiceConfiguration", _dataServiceConfiguration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options => 
                {
                    options.Authority = "https://identityserver.arantasar.hostingasp.pl/";
                    options.RequireHttpsMetadata = true;
                    options.ApiName = "api2";
                });

            services.AddCors(options =>
            {
                options.AddPolicy("RavenDataServiceApiPolicy", policy =>

                    policy.WithOrigins("https://smartscheduler.pl")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("X-Pagination")
                    .AllowCredentials()

                );
            });

            services.AddSingleton<ClientDocumentStoreHolder>();
            services.AddSingleton<IdentityServerDocumentStoreHolder>();
            services.AddSingleton(_dataServiceConfiguration);
            services.AddTransient<ISchedulerMailer>(serviceProvider => 
                new MailKitMailer(_dataServiceConfiguration.MailBoxHost, 
                _dataServiceConfiguration.MailBoxPortNumber, _dataServiceConfiguration.MailBoxUseSsl, 
                _dataServiceConfiguration.MailBoxAddress));
                

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<IUrlHelper, UrlHelper>(implementationFactory =>
            {
                var actionContext =
                    implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            IMapper mapper = mappingConfig.CreateMapper();
            services.AddSingleton(mapper);
            
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                options.HttpsPort = 443; 
               
            });
            
            services.AddMvc();
        }

        
        public void Configure(IApplicationBuilder app)
        {
            app.UseHttpsRedirection();
            app.UseCors("RavenDataServiceApiPolicy");
            app.ConfigureExceptionMiddleware(_logger);
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
