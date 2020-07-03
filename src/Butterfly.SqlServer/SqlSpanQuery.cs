using Butterfly.DataContract.Tracing;
using Butterfly.Storage;
using Butterfly.Storage.Query;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Butterfly.SqlServer
{
    public class SqlSpanQuery : DataAccessBase, ISpanQuery
    {
        public SqlSpanQuery(SqlServerOptions options) : base(options.ConnectionString)
        {

        }

        public async Task<Span> GetSpan(string spanId)
        {
            using (var conn = await GetConnectionAsync())
            {
                var span = new Span();

                var sqlSpan = "SELECT SpanId, TraceId, Sampled, OperationName, Duration, StartTimestamp, FinishTimestamp FROM Spans where SpanId = @SpanId";
                using (var cmd = new SqlCommand(sqlSpan, conn))
                {
                    cmd.Parameters.AddWithValue("SpanId", spanId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            return null;
                        }

                        await reader.ReadAsync();

                        span.SpanId = reader.GetString(0);
                        span.TraceId = reader.GetString(1);
                        span.Sampled = reader.GetBoolean(2);
                        span.OperationName = reader.GetString(3);
                        span.Duration = reader.GetInt64(4);
                        span.StartTimestamp = reader.GetDateTimeOffset(5);
                        span.FinishTimestamp = reader.GetDateTimeOffset(6);
                    }
                }

                span.Tags = await GetTags(spanId);
                span.Logs = await GetLogs(spanId);
                span.Baggages = await GetBaggages(spanId);
                span.References = await GetReferences(spanId);

                return span;
            }
        }

        private async Task<ICollection<Tag>> GetTags(string spanId)
        {
            var sql = "SELECT [Key], [Value] FROM Tags where SpanId = @SpanId";
            using (var conn = await GetConnectionAsync())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("SpanId", spanId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        var tags = new List<Tag>();

                        while (await reader.ReadAsync())
                        {
                            var tag = new Tag();
                            tag.Key = reader.GetString(0);
                            tag.Value = reader.GetString(1);
                            tags.Add(tag);
                        }

                        return tags;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private async Task<ICollection<Log>> GetLogs(string spanId)
        {
            var sql = "SELECT LogId, Timestamp FROM Logs where SpanId = @SpanId";
            using (var conn = await GetConnectionAsync())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("SpanId", spanId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        var logs = new List<Log>();

                        while (await reader.ReadAsync())
                        {
                            var log = new Log();
                            var logId = reader.GetGuid(0);
                            log.Timestamp = reader.GetDateTimeOffset(1);

                            log.Fields = await GetLogFields(logId);

                            logs.Add(log);
                        }

                        return logs;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private async Task<ICollection<LogField>> GetLogFields(Guid logId)
        {
            var sql = "SELECT [Key], [Value] FROM LogFields where LogId = @LogId";
            using (var conn = await GetConnectionAsync())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("LogId", logId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        var logFields = new List<LogField>();

                        while (await reader.ReadAsync())
                        {
                            var logField = new LogField();
                            logField.Key = reader.GetString(0);
                            logField.Value = reader.GetString(1);
                            logFields.Add(logField);
                        }

                        return logFields;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private async Task<ICollection<Baggage>> GetBaggages(string spanId)
        {
            var sql = "SELECT [Key], [Value] FROM Baggages where SpanId = @SpanId";
            using (var conn = await GetConnectionAsync())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("SpanId", spanId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        var baggages = new List<Baggage>();

                        while (await reader.ReadAsync())
                        {
                            var baggage = new Baggage();
                            baggage.Key = reader.GetString(0);
                            baggage.Value = reader.GetString(1);
                            baggages.Add(baggage);
                        }

                        return baggages;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private async Task<ICollection<SpanReference>> GetReferences(string spanId)
        {
            var sql = "SELECT Reference, ParentId FROM SpanReferences where SpanId = @SpanId";
            using (var conn = await GetConnectionAsync())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("SpanId", spanId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var references = new List<SpanReference>();

                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            var reference = new SpanReference();
                            reference.Reference = reader.GetString(0);
                            reference.ParentId = reader.GetString(1);
                            references.Add(reference);
                        }

                    }

                    return references;
                }
            }
        }

        public async Task<IEnumerable<Span>> GetSpanDependencies(DependencyQuery dependencyQuery)
        {
            var sql = "SELECT * FROM Spans ";

            if (dependencyQuery.StartTimestamp.HasValue || dependencyQuery.FinishTimestamp.HasValue)
            {
                sql += "WHERE ";
            }

            if (dependencyQuery.StartTimestamp.HasValue)
            {
                sql += "StartTimestamp >= @StartTimestamp ";
            }

            if (dependencyQuery.StartTimestamp.HasValue && dependencyQuery.FinishTimestamp.HasValue)
            {
                sql += "AND ";
            }

            if (dependencyQuery.FinishTimestamp.HasValue)
            {
                sql += "FinishTimestamp <= @FinishTimestamp";
            }

            var spans = new List<Span>();

            using (var conn = await GetConnectionAsync())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (dependencyQuery.StartTimestamp.HasValue)
                {
                    cmd.Parameters.AddWithValue("StartTimestamp", dependencyQuery.StartTimestamp);
                }

                if (dependencyQuery.FinishTimestamp.HasValue)
                {
                    cmd.Parameters.AddWithValue("FinishTimestamp", dependencyQuery.FinishTimestamp);
                }

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var span = new Span();
                        span.SpanId = reader.GetString(0);
                        span.TraceId = reader.GetString(1);
                        span.Sampled = reader.GetBoolean(2);
                        span.OperationName = reader.GetString(3);
                        span.Duration = reader.GetInt64(4);
                        span.StartTimestamp = reader.GetDateTimeOffset(5);
                        span.FinishTimestamp = reader.GetDateTimeOffset(6);

                        span.Tags = await GetTags(span.SpanId);
                        span.References = await GetReferences(span.SpanId);

                        spans.Add(span);
                    }
                }
                return spans;
            }


        }

        public async Task<Trace> GetTrace(string traceId)
        {
            var sql = "SELECT SpanId, TraceId, Sampled, OperationName, Duration, StartTimestamp, FinishTimestamp FROM Spans where TraceId = @TraceId";

            var spans = new List<Span>();

            using (var conn = await GetConnectionAsync())
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("TraceId", traceId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            return null;
                        }


                        while (await reader.ReadAsync())
                        {
                            var span = new Span();
                            span.SpanId = reader.GetString(0);
                            span.TraceId = reader.GetString(1);
                            span.Sampled = reader.GetBoolean(2);
                            span.OperationName = reader.GetString(3);
                            span.Duration = reader.GetInt64(4);
                            span.StartTimestamp = reader.GetDateTimeOffset(5);
                            span.FinishTimestamp = reader.GetDateTimeOffset(6);

                            spans.Add(span);
                        }
                    }
                }

                foreach (var span in spans)
                {

                    var sqlTags = "SELECT [Key], [Value] FROM Tags where SpanId = @SpanId";
                    using (var cmd = new SqlCommand(sqlTags, conn))
                    {

                        cmd.Parameters.AddWithValue("SpanId", span.SpanId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                var tags = new List<Tag>();

                                while (await reader.ReadAsync())
                                {
                                    var tag = new Tag()
                                    {
                                        Key = reader.GetString(0),
                                        Value = reader.GetString(1)
                                    };

                                    tags.Add(tag);
                                }

                                span.Tags = tags;
                            }
                        }
                    }

                    var references = await GetReferences(span.SpanId);
                    span.References = references;

                }


                var trace = new Trace();
                trace.TraceId = traceId;
                trace.Spans = spans;

                return trace;
            }
        }

        public async Task<IEnumerable<TraceHistogram>> GetTraceHistogram(TraceQuery traceQuery)
        {
            var sql = "select TOP (@Limit) cast(StartTimestamp as date) [Date], Datepart(hour, StartTimestamp) [Hour], DATEPART(minute, StartTimestamp) [Minute], Count(1)[Count]"
                    + " from Spans"
                    + " group by cast(StartTimestamp as date), Datepart(hour, StartTimestamp), DATEPART(minute, StartTimestamp)"
                    + " order by 1, 2, 3";

            using (var conn = await GetConnectionAsync())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("Limit", traceQuery.Limit);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    var histograms = new List<TraceHistogram>();

                    while (await reader.ReadAsync())
                    {
                        var histogram = new TraceHistogram();
                        histogram.Time = DateTimeOffset.Parse($"{reader.GetDateTime(0).ToShortDateString()} {reader.GetInt32(1):00}:{reader.GetInt32(2):00}");
                        histogram.Count = reader.GetInt32(3);

                        histograms.Add(histogram);
                    }

                    return histograms;
                }
            }
        }

        public async Task<IEnumerable<Trace>> GetTraces(TraceQuery traceQuery)
        {
            var sql = "SELECT TOP (@Limit) TraceId, MIN(StartTimestamp) as StartTimestamp, MIN(FinishTimestamp) as FinishTimestamp from Spans";

            if (traceQuery.StartTimestamp.HasValue || traceQuery.FinishTimestamp.HasValue)
            {
                sql += " WHERE";
            }

            if (traceQuery.StartTimestamp.HasValue)
            {
                sql += " StartTimestamp > @StartTimestamp";
            }

            if (traceQuery.StartTimestamp.HasValue && traceQuery.FinishTimestamp.HasValue)
            {
                sql += " AND";
            }

            if (traceQuery.FinishTimestamp.HasValue)
            {
                sql += " FinishTimestamp < @FinishTimestamp";
            }

            if ((traceQuery.StartTimestamp.HasValue || traceQuery.FinishTimestamp.HasValue) && !string.IsNullOrEmpty(traceQuery.ServiceName))
            {
                sql += " AND";
            }

            //TODO: Figure out how to filter on tags...
            //var tags = BuildQueryTags(traceQuery);

            if (!string.IsNullOrEmpty(traceQuery.ServiceName))
            {
                sql += " TraceId IN (SELECT s.traceid FROM Spans s JOIN Tags t ON s.SpanId = t.SpanId WHERE t.[Key] = 'service.name' AND t.Value = @ServiceName)";
            }

            sql += " GROUP BY TraceId"
                + " ORDER BY starttimestamp DESC";

            var traces = new List<Trace>();

            using (var conn = await GetConnectionAsync())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("Limit", traceQuery.Limit);

                if (traceQuery.StartTimestamp.HasValue)
                {
                    cmd.Parameters.AddWithValue("StartTimestamp", traceQuery.StartTimestamp.Value);
                }

                if (traceQuery.FinishTimestamp.HasValue)
                {
                    cmd.Parameters.AddWithValue("FinishTimestamp", traceQuery.FinishTimestamp.Value);
                }

                if (!string.IsNullOrEmpty(traceQuery.ServiceName))
                {
                    cmd.Parameters.AddWithValue("ServiceName", traceQuery.ServiceName);
                }

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var traceId = reader.GetString(0);

                        var trace = await GetTrace(traceId);

                        traces.Add(trace);
                    }
                }
                return traces;
            }
        }

        private List<Tag> BuildQueryTags(TraceQuery traceQuery)
        {
            var tagList = new List<Tag>();

            if (!string.IsNullOrEmpty(traceQuery.ServiceName))
            {
                tagList.Add(new Tag { Key = QueryConstants.Service, Value = traceQuery.ServiceName });
                return tagList;
            }

            if (!string.IsNullOrEmpty(traceQuery.Tags))
            {
                var tags = traceQuery.Tags.Split('|');
                foreach (var tag in tags)
                {
                    var pair = tag.Split('=');
                    if (pair.Length == 2)
                    {
                        tagList.Add(new Tag { Key = pair[0], Value = pair[1] });
                    }
                }
            }

            return tagList;
        }

    }
}
