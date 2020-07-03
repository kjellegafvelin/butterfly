using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Butterfly.DataContract.Tracing;

namespace Butterfly.SqlServer.Models
{
    [Table("SpanReferences")]
    public class SpanReferenceModel
    {     
        [Key]
        public long SpanReferenceId { get; set; }
        
        public string Reference { get; set; }
 
        public string SpanId { get; set; }

        public string ParentId { get; set; }

        internal static IEnumerable<SpanReferenceModel> Map(ICollection<SpanReference> references, string spanId)
        {
            var referenceModels = new List<SpanReferenceModel>();

            foreach (var reference in references)
            {
                var referenceModel = new SpanReferenceModel()
                {
                    SpanId = spanId,
                    ParentId = reference.ParentId,
                    Reference = reference.Reference
                };

                referenceModels.Add(referenceModel);
            }

            return referenceModels;
        }
    }
}