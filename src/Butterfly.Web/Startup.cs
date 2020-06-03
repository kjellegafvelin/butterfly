﻿using AutoMapper;
using Butterfly.Elasticsearch;
using Butterfly.EntityFrameworkCore;
using Butterfly.Consumer.Lite;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Butterfly.Common;
using Butterfly.HttpCollector;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace Butterfly.Server
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
            var mvcBuilder = services.AddMvc(option =>
            {
                option.OutputFormatters.Add(new MessagePackOutputFormatter(ContractlessStandardResolver.Options));
                option.InputFormatters.Add(new MessagePackInputFormatter(ContractlessStandardResolver.Options));
            });

            mvcBuilder.AddApplicationPart(typeof(HttpCollectorOptions).Assembly);

            services.AddResponseCompression();

            services.Configure<HttpCollectorOptions>(Configuration);

            services.AddCors();

            services.AddAutoMapper();

            services.AddSwaggerGen(option => { option.SwaggerDoc("v1", new OpenApiInfo { Title = "butterfly http api", Version = "v1" }); });

            services.AddLiteConsumer(Configuration)
                .AddEntityFrameworkCore(Configuration);

            services.AddElasticsearch(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "butterfly http api v1");
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseResponseCompression();

            app.UseCors(cors => cors.AllowAnyOrigin());
            
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(routes =>
            {
                routes.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                routes.MapFallbackToController("Index", "Home");
            });
        }
    }
}