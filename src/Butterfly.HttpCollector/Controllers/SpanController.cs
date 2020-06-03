using System.Threading;
using System.Threading.Tasks;
using Butterfly.Consumer;
using Butterfly.DataContract.Tracing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Butterfly.HttpCollector.Controllers
{
    [EnableHttpCollector]
    [Route("api/Span")]
    public class SpanController : Controller
    {
        private readonly ISpanProducer _spanProducer;

        public SpanController(ISpanProducer spanProducer)
        {
            _spanProducer = spanProducer;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Span[] spans)
        {
            if (spans != null)
            {
                await _spanProducer.PostAsync(spans, CancellationToken.None);
            }
            return StatusCode(StatusCodes.Status201Created);
        }
    }
}