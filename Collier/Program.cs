using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GrpcGreeter
{
    public class Program
    {
        //TODO look for existing process in the process list to prevent multiple miners from running.  It is possible to reattach to a process's stdout?

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
                });
        }
    }
}
