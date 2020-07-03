using Butterfly.DataContract.Tracing;
using Butterfly.SqlServer.Models;
using Butterfly.Storage;
using FastMember;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Butterfly.SqlServer
{
    public class SqlSpanStorage : DataAccessBase, ISpanStorage
    {
        public SqlSpanStorage(SqlServerOptions options) : base(options.ConnectionString)
        {
        }

        public async Task StoreAsync(IEnumerable<Span> spans, CancellationToken cancellationToken = default)
        {
            var spanModels = new List<SpanModel>();
            var tagModels = new List<TagModel>();
            var logModels = new List<LogModel>();
            var logFieldModels = new List<LogFieldModel>();
            var baggageModels = new List<BaggageModel>();
            var referenceModels = new List<SpanReferenceModel>();

            foreach (var span in spans)
            {
                spanModels.Add(SpanModel.Map(span));
                tagModels.AddRange(TagModel.Map(span.Tags, span.SpanId));

                var logModelsTemp = LogModel.Map(span.Logs, span.SpanId);
                logModels.AddRange(logModelsTemp);

                foreach (var logModel in logModelsTemp)
                {
                    logFieldModels.AddRange(logModel.Fields);
                }

                baggageModels.AddRange(BaggageModel.Map(span.Baggages, span.SpanId));
                referenceModels.AddRange(SpanReferenceModel.Map(span.References, span.SpanId));
            }

            try
            {
                using (var conn = await GetConnectionAsync())
                using (var bulkCopy = new SqlBulkCopy(conn))
                {
                    using (var reader = ObjectReader.Create(spanModels, "SpanId", "TraceId", "Sampled", "OperationName", "Duration", "StartTimestamp", "FinishTimestamp"))
                    {
                        bulkCopy.DestinationTableName = "Spans";

                        await bulkCopy.WriteToServerAsync(reader);
                    }

                    using (var reader = ObjectReader.Create(tagModels,"TagId", "SpanId", "Key", "Value"))
                    {
                        bulkCopy.DestinationTableName = "Tags";

                        await bulkCopy.WriteToServerAsync(reader);
                    }

                    using (var reader = ObjectReader.Create(logModels, "LogId", "SpanId", "Timestamp"))
                    {
                        bulkCopy.DestinationTableName = "Logs";

                        await bulkCopy.WriteToServerAsync(reader);
                    }

                    using (var reader = ObjectReader.Create(logFieldModels, "LogFieldId", "LogId", "Key", "Value"))
                    {
                        bulkCopy.DestinationTableName = "LogFields";

                        await bulkCopy.WriteToServerAsync(reader);
                    }

                    using (var reader = ObjectReader.Create(baggageModels, "BaggageId", "SpanId", "Key", "Value"))
                    {
                        bulkCopy.DestinationTableName = "Baggages";

                        await bulkCopy.WriteToServerAsync(reader);
                    }

                    using (var reader = ObjectReader.Create(referenceModels, "SpanReferenceId", "Reference", "SpanId", "ParentId"))
                    {
                        bulkCopy.DestinationTableName = "SpanReferences";

                        await bulkCopy.WriteToServerAsync(reader);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
