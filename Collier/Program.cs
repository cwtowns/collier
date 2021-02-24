using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace GrpcGreeter
{
    public class Program
    {
        //TODO can I keep color output?  I dislike my log right now and color output would be very nice.

        //TODO can it automatically pause when a process is detected?  that could be cool.  It doesn't look like this takes a lot of resources at all.
        //  There could be an auto mode and an override to manually pause.  When would I manually pause?  Almost never I would think, and I could auto unpause if I go idle for 20m.
        //  this would be way more efficient which would be cool. 

        /*
        //  service startup 
        //      at some point I will want to take over a process, this will require shutting it down gracefully via http
        //     
        //  gpu monitor will poll for running apps and fire an event if they are running 
        //  if no apps are running and load is low then we unpause.  this exists today.
        
        //  need another thing that looks for a gaming process and pauses the miner.
        //  
        //
        */

        //TODO add a feature that lets us specify what miner output we show at informational level so we can turn off some of the log spam 
        //this is probably going to be the rows that show hash rate and that's it?
        //ultimately my client is going to want to also stream log info from the entire application / get status remotely

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

        internal static string ENV_VARIABLE_COLLIER_ROOT_DIRECTORY = "COLLIER_ROOT_DIRECTORY";

        public static void Main(string[] args)
        {
            var rootDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            Environment.SetEnvironmentVariable(ENV_VARIABLE_COLLIER_ROOT_DIRECTORY, rootDirectory);

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
