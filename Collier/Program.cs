using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace GrpcGreeter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var rootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Environment.SetEnvironmentVariable(CollierServiceCollectionExtensions.ENV_VARIABLE_COLLIER_ROOT_DIRECTORY, rootDirectory);

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .UseWindowsService()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureLogging(log =>
                    {
                        log.AddSerilog(Log.Logger);
                    });

                    webBuilder.UseUrls("http://localhost:9999");  //TODO move to config file

                });
        }
    }
}
