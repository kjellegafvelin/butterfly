using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Butterfly.DataContract.Tracing;

namespace Butterfly.SqlServer.Models
{
    [Table("Tags")]
    public class TagModel
    {
        [Key]
        public long TagId { get; set; }
     
        public string SpanId { get; set; }
        
        public string Key { get; set; }

        public string Value { get; set; }

        public static IEnumerable<TagModel> Map(IEnumerable<Tag> tags, string spanId)
        {
            var tagModels = new List<TagModel>();

            foreach (var tag in tags)
            {
                var tagModel = new TagModel()
                {
                    SpanId = spanId,
                    Key = tag.Key,
                    Value = tag.Value
                };

                tagModels.Add(tagModel);
            }

            return tagModels;
        }

    }

}