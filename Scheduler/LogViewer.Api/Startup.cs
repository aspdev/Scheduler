using AutoMapper;
using LogViewer.Api.Config;
using LogViewer.Api.Entities;
using LogViewer.Api.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;

namespace LogViewer.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly LogViewerConfiguration _logViewerConfiguration;
        private readonly ElasticClient _elasticClient;
        

        public Startup(IHostingEnvironment environment)
        {
            var builder = new ConfigurationBuilder();
            builder.SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true);

            _configuration = builder.Build();
            _logViewerConfiguration = new LogViewerConfiguration();
            _configuration.Bind("LogViewer", _logViewerConfiguration);


            _elasticClient = new ElasticClient(new Uri(_logViewerConfiguration.ElasticsearchUri));

        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("LogViewerPolicy", policy =>
                    policy.WithOrigins("http://localhost:8080","http://localhost:8082")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("X-Pagination")
                    .AllowCredentials());
            });

            services.AddSingleton(_logViewerConfiguration);
            services.AddSingleton(_elasticClient);
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<IUrlHelper, UrlHelper>(implementationFactory =>
            {
                var actionContext =
                    implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<IHit<Logevent>, LogeventDto>()
                .ForMember(e => e.LogeventId, opt => opt.MapFrom(src => src.Id))
                .ForMember(e => e.Level, opt => opt.MapFrom(src => src.Source.Level))
                .ForMember(e => e.Logger, opt => opt.MapFrom(src => src.Source.Logger))
                .ForMember(e => e.Message, opt => opt.MapFrom(src => src.Source.Message))
                .ForMember(e => e.Date, opt => opt.MapFrom(src => src.Source.Timestamp.ToShortDateString()))
                .ForMember(e => e.Time, opt => opt.MapFrom(src => src.Source.Timestamp.ToLocalTime().TimeOfDay))
                .ForMember(e => e.Appbasepath, opt => opt.MapFrom(src => src.Source.Appbasepath));
            });

            app.UseCors("LogViewerPolicy");
            app.UseMvc();
        }
    }
}
