// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.IO;
using IdentityServer.DataStore;
using IdentityServer.Extentions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scheduler.Mailer.Interfaces;
using Scheduler.Mailer.MailKit;

namespace IdentityServer
{
    public class Startup
    {
        private IConfiguration Configuration { get; set; }
        private IHostingEnvironment Environment { get; }
        private readonly IdentityServerConfiguration _identityServerConfiguration;

        public Startup(IHostingEnvironment environment, ILogger<Startup> logger)
        {
            Environment = environment;
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true);
            Configuration = configurationBuilder.Build();
            
            _identityServerConfiguration = new IdentityServerConfiguration();
            Configuration.Bind("IdentityServerConfiguration", _identityServerConfiguration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
           services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            services.AddSingleton<IDocumentStoreHolder, DocumentStoreHolder>();
            
            services.AddTransient<ISchedulerMailer>(serviceProvider => 
                new MailKitMailer(_identityServerConfiguration.MailService.Host, 
                    _identityServerConfiguration.MailService.Port, 
                    _identityServerConfiguration.MailService.UseSsl, 
                    _identityServerConfiguration.MailService.MailBoxAddress));

            var certificatePath = Path.Combine(Environment.ContentRootPath, "Certificates", "IdentityServerCert.pfx");

            var builder = services.AddIdentityServer()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Config.GetClients())
                .AddCustomUserStore()
                .LoadSigningCredentialsFrom(certificatePath,
                    _identityServerConfiguration.Certificates.TokenCertificatePassword);
            
           services.AddSingleton(_identityServerConfiguration);
           services.AddHttpsRedirection(options =>
           {
               options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
               options.HttpsPort = 443; 
               
           });

        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseIdentityServer();

            app.UseMvcWithDefaultRoute();
        }
    }
}