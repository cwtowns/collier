using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace GrpcGreeter
{
    public class Program
    {
        public static IConfiguration Config { get; private set; }
        public static void Main(string[] args)
        {
            var rootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Environment.SetEnvironmentVariable(CollierServiceCollectionExtensions.ENV_VARIABLE_COLLIER_ROOT_DIRECTORY, rootDirectory);

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var rootDirectory = Environment.GetEnvironmentVariable(CollierServiceCollectionExtensions.ENV_VARIABLE_COLLIER_ROOT_DIRECTORY) ??
                                throw new ArgumentNullException("env." + CollierServiceCollectionExtensions.ENV_VARIABLE_COLLIER_ROOT_DIRECTORY,
                                    "This is normally set during Main execution.");

            Config = new ConfigurationBuilder()
                .SetBasePath(rootDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("minersettings.private.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

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

                    webBuilder.UseUrls("http://localhost:" + Config["serverPort"]);
                });
        }
    }
}
