using Butterfly.DataContract.Tracing;
using Butterfly.Storage;
using Butterfly.Storage.Query;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Butterfly.SqlServer
{
    public class SqlServiceQuery : DataAccessBase, IServiceQuery
    {
        public SqlServiceQuery(SqlServerOptions options) : base(options.ConnectionString)
        {

        }

        public async Task<IEnumerable<Service>> GetServices(TimeRangeQuery query)
        {
            var sql = "SELECT DISTINCT [Value] FROM Tags where [Key] = 'service.name'";
            using (var conn = await GetConnectionAsync())
            using (var cmd = new SqlCommand(sql, conn))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        var services = new List<Service>();

                        while (await reader.ReadAsync())
                        {
                            var service = new Service();
                            service.Name = reader.GetString(0);
                            services.Add(service);
                        }

                        return services;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}
