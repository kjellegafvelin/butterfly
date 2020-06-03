using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Butterfly.Common
{
    public class MessagePackOutputFormatter : IOutputFormatter //, IApiResponseTypeMetadataProvider
    {
        const string ContentType = "application/x-msgpack";
        static readonly string[] SupportedContentTypes = new[] { ContentType };

        private readonly MessagePackSerializerOptions options;

        public MessagePackOutputFormatter()
            : this(null)
        {

        }
        public MessagePackOutputFormatter(MessagePackSerializerOptions options)
        {
            this.options = options ?? MessagePackSerializer.DefaultOptions;
        }

        public bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            return ContentType == context.HttpContext.Request.ContentType;
        }

        public Task WriteAsync(OutputFormatterWriteContext context)
        {
            context.HttpContext.Response.ContentType = ContentType;

            // 'object' want to use anonymous type serialize, etc...
            if (context.ObjectType == typeof(object))
            {
                if (context.Object == null)
                {
                    context.HttpContext.Response.Body.WriteByte(MessagePackCode.Nil);
                    return Task.CompletedTask;
                }
                else
                {
                    // use concrete type.
                    return MessagePackSerializer.SerializeAsync(context.Object.GetType(), context.HttpContext.Response.Body, context.Object, options);
                }
            }
            else
            {
                return MessagePackSerializer.SerializeAsync(context.ObjectType, context.HttpContext.Response.Body, context.Object, options);
            }
        }
    }

    public class MessagePackInputFormatter : IInputFormatter // , IApiRequestFormatMetadataProvider
    {
        private const string ContentType = "application/x-msgpack";
        private static readonly string[] SupportedContentTypes = new[] { ContentType };

        private readonly MessagePackSerializerOptions options;

        public MessagePackInputFormatter()
            : this(null)
        {

        }

        public MessagePackInputFormatter(MessagePackSerializerOptions options)
        {
            this.options = this.options ?? MessagePackSerializer.DefaultOptions;
        }

        public bool CanRead(InputFormatterContext context)
        {
            return ContentType == context.HttpContext.Request.ContentType;
        }

        public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            var result = await MessagePackSerializer.DeserializeAsync(context.ModelType, request.Body, options);
            return await InputFormatterResult.SuccessAsync(result);
        }
    }
}
