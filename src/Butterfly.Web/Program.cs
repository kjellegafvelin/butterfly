using Butterfly.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Butterfly.Server
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var processRoot = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var contentRoot = WindowsServiceHelpers.IsWindowsService()
                ? processRoot : Directory.GetCurrentDirectory();


            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.SpaServices", Serilog.Events.LogEventLevel.Information)
                .MinimumLevel.Override("SoapCore", Serilog.Events.LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.RollingFile(Path.Combine(processRoot, "logs\\Butterfly-server.txt"), fileSizeLimitBytes: 10 * 1024 * 1024)
                .CreateLogger();

            Log.Information("----------------------------------------------------------------------");

            var assemblyName = Assembly.GetExecutingAssembly().GetName();
            var fileInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            Log.Information($"{assemblyName.Name} v{fileInfo.ProductVersion}");

            try
            {
                CreateHostBuilder(args, contentRoot)
                    .UseWindowsService()
                    .Build().Run();
                return 0;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args, string contentRoot) =>
            Host.CreateDefaultBuilder()
            .UseSerilog()
            .UseContentRoot(contentRoot)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>()
                    .UseUrls(AddressHelpers.GetApplicationUrl(args))
                    .UseWebRoot(Path.Combine(contentRoot, "wwwroot"));
            });

    }
}