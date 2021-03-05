using System;
using System.Data.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Collier.IO;
using Collier.Mining;
using Collier.Monitoring;
using Collier.Monitoring.Gpu;
using Collier.Monitoring.Idle;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using Collier.Host;
using CollierService.Mining;
using CollierService.Monitoring.Gpu;
using Serilog;
using GpuMonitoringBackgroundService = Collier.Monitoring.Gpu.GpuMonitoringBackgroundService;

namespace GrpcGreeter
{

    /**
     * TODO ux notes are here
     *
     * It should display the log so I can see activity (max lines are 86 chars long or more)
     * It should show state (mining, idle, service not running)
     * It should allow state change (manual pause or stop)
     * It should show some summary info maybe like the hash rate, power usage, temp, crashes.
     * long press on the log to get more details and have it take over the entire display.  That'd be neat.
     *
     */
    public class Startup
    {
        private IServiceCollection _services;
        private IConfiguration _configuration;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        // This method gets called by the runtime. Use this method to add _services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            _services = services;

            var rootDirectory = Environment.GetEnvironmentVariable(CollierServiceCollectionExtensions.ENV_VARIABLE_COLLIER_ROOT_DIRECTORY) ??
                                throw new ArgumentNullException("env." + CollierServiceCollectionExtensions.ENV_VARIABLE_COLLIER_ROOT_DIRECTORY,
                                    "This is normally set during Main execution.");

            _configuration = new ConfigurationBuilder()
                .SetBasePath(rootDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("minersettings.private.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            //https://developer.okta.com/blog/2019/11/21/csharp-websockets-tutorial
            _services.AddSignalRCore();

            //_services.AddGrpc();
            _services.AddCollier(_configuration, _cancellationTokenSource);

            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(_configuration).CreateLogger();
        }

        private void OnShutdown()
        {
            _cancellationTokenSource.Cancel();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Microsoft.Extensions.Hosting.IHostApplicationLifetime lifetime)
        {
            lifetime.ApplicationStopping.Register(OnShutdown);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            /*
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
            */
        }
    }
}

