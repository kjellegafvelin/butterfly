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
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseResponseCompression();

            app.UseCors(cors => cors.AllowAnyOrigin());

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(routes =>
            {
                _ = routes.MapControllers();

                _ = routes.MapFallbackToFile("/index.html");
            });
        }
    }
}