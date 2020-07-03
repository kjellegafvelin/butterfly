using Butterfly.DataContract.Tracing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Butterfly.SqlServer.Models
{
    [Table("LogFields")]
    public class LogFieldModel
    {
        [Key]
        public long LogFieldId { get; set; }

        public Guid LogId { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }

        internal static IEnumerable<LogFieldModel> Map(ICollection<LogField> fields, Guid logId)
        {
            var logFieldModels = new List<LogFieldModel>();

            foreach (var logField in fields)
            {
                var logFieldModel = new LogFieldModel();
                logFieldModel.LogId = logId;
                logFieldModel.Key = logField.Key;
                logFieldModel.Value = logField.Value;

                logFieldModels.Add(logFieldModel);
            }

            return logFieldModels;
        }
    }
}