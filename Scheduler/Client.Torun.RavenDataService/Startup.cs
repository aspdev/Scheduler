using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
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

        private readonly DataServiceConfiguration _dataServiceConfiguration;
        private readonly IConfiguration _configuration;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

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
                    options.Authority = "https://localhost:44349";
                    options.RequireHttpsMetadata = true;
                    options.ApiName = "api2";
                });

            services.AddCors(options =>
            {
                options.AddPolicy("RavenDataServiceApiPolicy", policy =>

                    policy.WithOrigins("http://localhost:8081", "http://localhost:44388")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders("X-Pagination")
                    .AllowCredentials()

                );


            });


            services.AddSingleton<IDocumentStoreHolder, DocumentStoreHolder>();
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
            
            services.AddMvc();
        }

        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<Startup>();
            app.UseCors("RavenDataServiceApiPolicy");
            app.ConfigureExceptionMiddleware(logger, env);
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
