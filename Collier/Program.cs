using System;
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
        //TODO we can't pause the miner when we want to game, we have to stop it.
        //the DAG stays loaded and chews up a chunk of ram that's needed to get good performance during gaming

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

        //TOOD how does running as a service work?
        //https://mcilis.medium.com/how-to-publish-net-core-grpc-server-as-a-windows-service-dd562a1e263d
        //puslishing models
        //https://docs.microsoft.com/en-us/dotnet/core/deploying/
        //creating the installer using WIX
        //https://nblumhardt.com/2017/04/netcore-msi/
        //once I set this up I can run the dev platform with a fake process that acts like the miner so I can iterate on this
        //while still mining via my installed service

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
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
