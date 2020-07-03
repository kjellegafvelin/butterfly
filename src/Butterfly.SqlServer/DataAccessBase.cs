using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Butterfly.SqlServer
{
    public abstract class DataAccessBase
    {
        private readonly string connectionString;

        protected DataAccessBase(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            this.connectionString = connectionString;
        }

        protected async Task<SqlConnection> GetConnectionAsync()
        {
            var conn = new SqlConnection(this.connectionString);
            
            await conn.OpenAsync();

            return conn;
        }
    }
}
