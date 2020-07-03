using Butterfly.DataContract.Tracing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Butterfly.SqlServer.Models
{
    [Table("Baggages")]
    public class BaggageModel
    {
        [Key]
        public long BaggageId { get; set; }
           
        public string SpanId { get; set; }
        
        public string Key { get; set; }

        public string Value { get; set; }

        internal static IEnumerable<BaggageModel> Map(ICollection<Baggage> baggages, string spanId)
        {
            var baggageModels = new List<BaggageModel>();

            foreach (var baggage in baggages)
            {
                var baggageModel = new BaggageModel()
                {
                    SpanId = spanId,
                    Key = baggage.Key,
                    Value = baggage.Value
                };

                baggageModels.Add(baggageModel);
            }

            return baggageModels;
        }
    }
}