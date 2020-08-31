using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Butterfly.Server.Controllers
{
    //public class HomeController : Controller
    //{
    //    // GET
    //    public IActionResult Index([FromServices] IWebHostEnvironment env, [FromServices] ILogger<HomeController> logger)
    //    {
    //        logger.LogInformation($"Web root path: { env.WebRootPath }");
    //        logger.LogInformation($"Content root path: { env.ContentRootPath }");

    //        using (var stream = System.IO.File.OpenRead(Path.Combine(env.WebRootPath, "index.html")))
    //        {
    //            using (var reader = new StreamReader(stream))
    //            {
    //                return Content(reader.ReadToEnd(), "text/html");
    //            }
    //        }
    //    }
    //}
}