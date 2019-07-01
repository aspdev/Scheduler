using Client.Torun.DataService.Config;
using Client.Torun.DataService.Extentions;
using Client.Torun.DataService.Interfaces;
using Client.Torun.DataService.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Client.Torun.DataService
{
    public class Startup
    {

        private readonly IConfiguration _configuration;
        private readonly DataServiceConfiguration _dataServiceConfiguration;
        


        public Startup(IHostingEnvironment env)
        {
            env.ConfigureNLog("Nlog.config");
           
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
               
            _configuration = builder.Build();


            _dataServiceConfiguration = new DataServiceConfiguration();
            _configuration.Bind("DataService", _dataServiceConfiguration);



        }
        
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddDbContext<DataServiceContext>(options => options.UseSqlServer(_dataServiceConfiguration.ConnectionString));
            services.AddSingleton(_dataServiceConfiguration);

            services.AddScoped<IRepository>(provider =>
                new DataServiceRepository(provider.GetService<DataServiceContext>()));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddMvc();

        }

        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
           
            var logger = loggerFactory.CreateLogger<Startup>();

            app.ConfigureExceptionMiddleware(logger, env);

            app.UseMvc();


        }
    }
}
