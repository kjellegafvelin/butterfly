using Butterfly.DataContract.Tracing;
using Butterfly.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Butterfly.SqlServer
{
    public class SqlServiceStorage : IServiceStorage
    {
        public Task StoreServiceAsync(IEnumerable<Service> services, CancellationToken cancellationToken)
        {
            // Don't know the purpose of this method yet.
            return Task.CompletedTask;
        }
    }
}
