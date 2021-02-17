using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiningAutomater.Host;
using MiningAutomater.IO;
using MiningAutomater.Mining;
using MiningAutomater.Monitoring;
using MiningAutomater.Monitoring.Gpu;
using MiningAutomater.Monitoring.Idle;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace GrpcGreeter
{
    public class Startup
    {
        private IServiceCollection _services;
        private IConfiguration _configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            _services = services;

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("minersettings.private.json", optional: false)
                .AddEnvironmentVariables()
                .Build();


            services.AddGrpc();



            //services.Configure<GpuMonitoringSettings>(_configuration.GetSection("gpuMonitoring"));

            var monitoringSection = _configuration.GetSection("monitoring");
            var tRexMinerSection = _configuration.GetSection("miner").GetSection("t-rex");

            services.Configure<TrexMiner.Settings>(options => tRexMinerSection.Bind(options));
            services.Configure<TrexWebClient.Settings>(options => tRexMinerSection.GetSection("web").Bind(options));

            services.Configure<MiningAutomater.Monitoring.EventCoordinatorBackgroundService.Settings>(options => monitoringSection.GetSection("eventCoordinator").Bind(options));

            var gpuMonitoring = monitoringSection.GetSection("gpuMonitoring");

            services.Configure<MiningAutomater.Monitoring.Gpu.GpuMonitoringBackgroundService.Settings>(options => gpuMonitoring.Bind(options));

            services.Configure<GpuMonitorOutputParser_GpuLoad.Settings>(options => gpuMonitoring.GetSection("outputParsers").GetSection("gpuLoadOutputParser").Bind(options));
            services.Configure<GpuMonitorOutputParser_ProcessList.Settings>(options => gpuMonitoring.GetSection("outputParsers").GetSection("processListOutputParser").Bind(options));

            services.Configure<NvidiaSmiExecutor.Settings>(options => gpuMonitoring.Bind(options));


            services.Configure<IdleMonitor.Settings>(options => monitoringSection.GetSection("userIdle").Bind(options));
            services.Configure<IdleMonitorBackgroundService.Settings>(options => monitoringSection.GetSection("userIdle").Bind(options));




            services.AddSingleton<IMiner, TrexMiner>();
            services.AddSingleton<IGpuMonitorOutputParser, GpuMonitorOutputParser_GpuLoad>();
            services.AddSingleton<IGpuMonitorOutputParser, GpuMonitorOutputParser_ProcessList>();
            services.AddSingleton<ITrexWebClient, TrexWebClient>();
            services.AddSingleton<IEventCoordinatorBackgroundService, MiningAutomater.Monitoring.EventCoordinatorBackgroundService>();
            services.AddSingleton<IGpuMonitoringBackgroundService, MiningAutomater.Monitoring.Gpu.GpuMonitoringBackgroundService>();
            services.AddSingleton<IIdleMonitorBackgroundService, IdleMonitorBackgroundService>();
            services.AddSingleton<IIdleMonitor, IdleMonitor>();
            services.AddSingleton<ProcessFactory, ProcessFactory>();
            services.AddSingleton<INvidiaSmiParser, NvidiaSmiParser>();
            services.AddSingleton<INvidiaSmiExecutor, NvidiaSmiExecutor>();
            services.AddSingleton<IGpuMonitor, GpuMonitor>();
            services.AddSingleton(new HttpClient());

            //services.AddSingleton<IBackgroundService<GpuMonitoringBackgroundService>, GpuMonitoringBackgroundService>();
            //services.AddSingleton<IBackgroundService<IdleMonitorBackgroundService>, IdleMonitorBackgroundService>();
            //services.AddSingleton<IBackgroundService<EventCoordinatorBackgroundService>, EventCoordinatorBackgroundService>();

            services.AddHostedService<MiningAutomater.Host.GpuMonitoringBackgroundService>();
            services.AddHostedService<MiningAutomater.Host.IdleMonitoringBackgroundService>();
            services.AddHostedService<MiningAutomater.Host.EventCoordinatorBackgroundService>();

            //TODO why doesnt this approach work?  Make a sample project and post on stack overflow?

            //services.AddHostedService<BackgroundServiceHelper<GpuMonitoringBackgroundService>>();
            //services.AddHostedService<BackgroundServiceHelper<IdleMonitorBackgroundService>>();
            //services.AddHostedService<BackgroundServiceHelper<EventCoordinatorBackgroundService>>();
            /*
             *      //services.AddHostedService<GpuMonitoringBackgroundService>();
             *      //services.AddHostedService<IdleMonitorBackgroundService>();
             *      
             *      If we register the services themselves with the DI container as singletons types
             *      and also as services, we will not get singleton behavior.  Apparently
             *      we have to delegate control of the sub services to the parent service and only
             *      add the parent service as a hosted service.  
             *      
             *      https://github.com/dotnet/extensions/issues/553#issuecomment-462892616
             *      
             *      basically says do not depend on the service itself, depend on a different abstraction
             *      this means event Coordinator background service needs to depend on something else for GPU and Idle background service.
             *      
             *      //TODO remove the weirdness of how I'm doing background service injection
             */


            services.AddLogging(options =>
            {
                options.AddSimpleConsole(c =>
                {
                    c.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
                    // c.UseUtcTimestamp = true; // something to consider
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}

