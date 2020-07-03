using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Butterfly.DataContract.Tracing;

namespace Butterfly.SqlServer.Models
{
    [Table("Logs")]
    public class LogModel
    {
        [Key]
        public Guid LogId { get; set; }
        
        public string SpanId { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public List<LogFieldModel> Fields { get; set; }

        internal static IEnumerable<LogModel> Map(ICollection<Log> logs, string spanId)
        {
            var logModels = new List<LogModel>();

            foreach (var log in logs)
            {
                var logModel = new LogModel();
                var logId = Guid.NewGuid();
                logModel.LogId = logId;
                logModel.SpanId = spanId;
                logModel.Timestamp = log.Timestamp;

                logModel.Fields = new List<LogFieldModel>();
                logModel.Fields.AddRange(LogFieldModel.Map(log.Fields, logId));

                logModels.Add(logModel);
            }

            return logModels;
        }
    }
}