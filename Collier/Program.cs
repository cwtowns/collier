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
        //TODO how does running as a service work?
        //https://mcilis.medium.com/how-to-publish-net-core-grpc-server-as-a-windows-service-dd562a1e263d
        //puslishing models
        //https://docs.microsoft.com/en-us/dotnet/core/deploying/
        //if i dont want my users to install the framework themselves then I am publishing a self contained executable.  This requires a RID.
        //downside to self contained is I have to manually update when I want to support new versions (to get new security updates).
        //so lets do framework dependent i guess
        //https://docs.microsoft.com/en-us/windows/msix/desktop/vs-package-overview
        //this says the preferred way to install is MSIX
        //not clear if msix apps can accces other apps.  it goes against the preferred approach of misx to separate app state from system state.
        //https://www.advancedinstaller.com/msix-windows-services.html
        //https://stackoverflow.com/questions/9021075/how-to-create-an-installer-for-a-net-windows-service-using-visual-studio
        //https://github.com/iswix-llc/iswix-tutorials
        //manually install the service:
        //https://docs.microsoft.com/en-us/dotnet/framework/windows-services/how-to-install-and-uninstall-services
        //which says in an elevated command prompt do:  sc create Collier binpath=C:\Users\hodge\Documents\Collier\CollierService.exe
        //which I do but I'm unable to start it, it does not respond in a timely fashion
        //event viewer shows Exception Info: System.IO.FileNotFoundException: The configuration file 'appsettings.json' was not found and is not optional. The physical path is 'C:\Windows\system32\appsettings.json'.
        //i could resolve this by either putting my stuff in appdata and looking there, or changing directory to the executable location?

        //when running as an installed local service installed via sc the service writes logs to:
        //C:\Windows\System32\config\systemprofile\AppData\Local\Collier\log\
        //that is very difficult to figure out (had to use procmon)

        //creating the installer using WIX
        //https://nblumhardt.com/2017/04/netcore-msi/
        //once I set this up I can run the dev platform with a fake process that acts like the miner so I can iterate on this
        //while still mining via my installed service

        public static void Main(string[] args)
        {
            var rootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Environment.SetEnvironmentVariable(CollierServiceCollectionExtensions.ENV_VARIABLE_COLLIER_ROOT_DIRECTORY, rootDirectory);

            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureLogging(log =>
                    {
                        log.AddSerilog(Log.Logger);
                    });
                });
        }
    }
}
