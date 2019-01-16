using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FishingMap.Domain.Interfaces;
using FishingMap.Domain.Services;
using FishingMap.Domain.Data.Context;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;

namespace FishingMap.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddAutoMapper();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var connectionString = Configuration.GetConnectionString("FishingMapDatabase");

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString, opt => opt.UseNetTopologySuite()), ServiceLifetime.Scoped);

            services.AddScoped<IGeometryFactory>(provider => new GeometryFactory(new PrecisionModel(), 4326));
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<ILocationOwnerService, LocationOwnerService>();
            services.AddScoped<ISpeciesService, SpeciesService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
            );

            app.UseMvc();

            
        }
    }
}
