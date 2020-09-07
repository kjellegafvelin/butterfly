using AutoMapper;
using Butterfly.Consumer.Lite;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Butterfly.Common;
using Butterfly.HttpCollector;
using Microsoft.Extensions.Hosting;
using Butterfly.SqlServer.Extensions;
using Butterfly.Web.Common;
using Microsoft.AspNetCore.SpaServices.AngularCli;

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
            var butterflyOptions = Configuration.GetSection("Butterfly");
            services.Configure<ButterflyOptions>(butterflyOptions);

            var mvcBuilder = services.AddControllers(options =>
            {
                options.OutputFormatters.Add(new MessagePackOutputFormatter(ContractlessStandardResolver.Options));
                options.InputFormatters.Add(new MessagePackInputFormatter(ContractlessStandardResolver.Options));
            });

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            mvcBuilder.AddApplicationPart(typeof(HttpCollectorOptions).Assembly);

            services.AddResponseCompression();

            services.Configure<HttpCollectorOptions>(Configuration);

            services.AddCors();

            services.AddAutoMapper();

            services.AddLiteConsumer(Configuration);

            services.AddSqlServer(options =>
            {
                options.ConnectionString = Configuration.GetConnectionString("ButterflyDb");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseResponseCompression();

            app.UseCors(cors => cors.AllowAnyOrigin());

            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseEndpoints(routes =>
            {
                routes.MapControllers();

                routes.MapFallbackToFile("/index.html");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}