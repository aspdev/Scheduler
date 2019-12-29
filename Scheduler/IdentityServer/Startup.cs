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

namespace IdentityServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }
        public IHostingEnvironment Environment { get; }

        public Startup(IHostingEnvironment environment, ILogger<Startup> logger)
        {
            Environment = environment;
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true);
            Configuration = configurationBuilder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
           services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            services.AddSingleton<IDocumentStoreHolder, DocumentStoreHolder>();

            var certificatePath = Path.Combine(Environment.ContentRootPath, "Certificates", "IdentityServerCert.pfx");
            

            var builder = services.AddIdentityServer()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Config.GetClients())
                .AddCustomUserStore()
                .LoadSigningCredentialsFrom(certificatePath,
                    Configuration.GetSection("Certificates")["Token.Certificate.Password"]);
                
           services.AddSingleton(Configuration);

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

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseIdentityServer();

            app.UseMvcWithDefaultRoute();
        }
    }
}