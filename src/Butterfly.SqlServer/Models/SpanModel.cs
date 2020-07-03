using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Butterfly.DataContract.Tracing;

namespace Butterfly.SqlServer.Models
{
    [Table("Spans")]
    public class SpanModel
    {
        [Key]
        public string SpanId { get; set; }

        public string TraceId { get; set; }

        public bool Sampled { get; set; }

        public string OperationName { get; set; }

        public long Duration { get; set; }

        public DateTimeOffset StartTimestamp { get; set; }

        public DateTimeOffset FinishTimestamp { get; set; }

        public List<LogModel> Logs { get; set; }

        public List<TagModel> Tags { get; set; }

        public List<BaggageModel> Baggages { get; set; }

        public List<SpanReferenceModel> References { get; set; }

        public static SpanModel Map(Span span)
        {
            var spanModel = new SpanModel()
            {
                SpanId = span.SpanId,
                TraceId = span.TraceId,
                Sampled = span.Sampled,
                OperationName = span.OperationName,
                Duration = span.Duration,
                StartTimestamp = span.StartTimestamp,
                FinishTimestamp = span.FinishTimestamp
            };

            return spanModel;
        }
    }
}