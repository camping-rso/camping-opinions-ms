using CampingOpinionsAPI.Models;
using CampingOpinionsAPI.Services;
using CampingOpinionsAPI.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace CampingOpinionsAPI
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
            services.AddControllers().AddNewtonsoftJson(x => x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            // get connection string from env var
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

            // DB models service
            services.AddEntityFrameworkNpgsql().AddDbContext<avtokampiContext>(options =>
                options.UseNpgsql(connectionString)
            );

            // DB models service
            //services.AddEntityFrameworkNpgsql().AddDbContext<avtokampiContext>(options =>
            //    options.UseNpgsql(Configuration.GetConnectionString("Avtokampi"))
            //);

            // Repository services
            services.AddScoped<IMnenjaRepository, MnenjaRepository>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddRouting(options => options.LowercaseUrls = true);

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Camping opinions Web API",
                    Version = "v1",
                    Description = "Web API for opinions on camps"
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Avtokampi");
                c.RoutePrefix = string.Empty;
            });

            app.UseCors(
                options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
            );

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
