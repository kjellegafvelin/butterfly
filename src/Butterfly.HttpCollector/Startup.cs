using Butterfly.Common;
using Butterfly.Consumer.Lite;
using Butterfly.Elasticsearch;
using Butterfly.EntityFrameworkCore;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Butterfly.HttpCollector
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

            services.Configure<HttpCollectorOptions>(option =>
            {
                option.EnableHttpCollector = true;
            });

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
            }

            app.UseRouting();
        }
    }
}