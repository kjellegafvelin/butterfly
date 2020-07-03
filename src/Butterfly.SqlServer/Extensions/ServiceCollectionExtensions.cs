using Butterfly.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Butterfly.SqlServer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlServer(this IServiceCollection services, Action<SqlServerOptions> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var options = new SqlServerOptions();

            configure(options);

            services.AddSingleton<SqlServerOptions>(options);

            services.AddScoped<ISpanStorage, SqlSpanStorage>();
            services.AddScoped<ISpanQuery, SqlSpanQuery>();
            services.AddScoped<IServiceQuery, SqlServiceQuery>();
            services.AddSingleton<IServiceStorage, SqlServiceStorage>();

            return services;
        }
    }
}
