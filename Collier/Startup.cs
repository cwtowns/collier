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
using CollierService.Monitoring.Gpu;
using Serilog;
using GpuMonitoringBackgroundService = Collier.Monitoring.Gpu.GpuMonitoringBackgroundService;

namespace GrpcGreeter
{
    public class Startup
    {
        private IServiceCollection _services;
        private IConfiguration _configuration;
        private IWebHostEnvironment _hostEnv;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        // This method gets called by the runtime. Use this method to add _services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            _services = services;

            var rootDirectory = Environment.GetEnvironmentVariable(Program.ENV_VARIABLE_COLLIER_ROOT_DIRECTORY) ??
                                throw new ArgumentNullException("env." + Program.ENV_VARIABLE_COLLIER_ROOT_DIRECTORY,
                                    "This is normally set during Main execution.");

            _configuration = new ConfigurationBuilder()
                .SetBasePath(rootDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("minersettings.private.json", optional: false)
                .AddEnvironmentVariables()
                .Build();


            _services.AddGrpc();

            //TODO long term look for a framework that automates this binding.  this is a lot of boilerplate code

            //_services.Configure<GpuMonitoringSettings>(_configuration.GetSection("gpuMonitoring"));

            var monitoringSection = _configuration.GetSection("monitoring");
            var tRexMinerSection = _configuration.GetSection("miner").GetSection("t-rex");

            _services.Configure<TrexMiner.Settings>(options => tRexMinerSection.Bind(options));
            _services.Configure<TrexWebClient.Settings>(options => tRexMinerSection.GetSection("web").Bind(options));


            var gpuMonitoring = monitoringSection.GetSection("gpuMonitoring");

            _services.Configure<Collier.Monitoring.Gpu.GpuMonitoringBackgroundService.Settings>(options => gpuMonitoring.Bind(options));

            _services.Configure<GpuMonitorOutputParser_ProcessList.Settings>(options => gpuMonitoring.GetSection("outputParsers").GetSection("processListOutputParser").Bind(options));

            _services.Configure<NvidiaSmiExecutor.Settings>(options => gpuMonitoring.Bind(options));


            _services.Configure<IdleMonitor.Settings>(options => monitoringSection.GetSection("userIdle").Bind(options));
            _services.Configure<IdleMonitorBackgroundService.Settings>(options => monitoringSection.GetSection("userIdle").Bind(options));


            //TODO refactor the DI wire up to follow dot net core best practices via extension methods

            _services.AddSingleton<IMiner, TrexMiner>();
            _services.AddSingleton<IGpuMonitorOutputParser, GpuMonitorOutputParser_ProcessList>();
            _services.AddSingleton<ITrexWebClient, TrexWebClient>();
            _services.AddSingleton<IGpuMonitoringBackgroundService, Collier.Monitoring.Gpu.GpuMonitoringBackgroundService>();
            _services.AddSingleton<IGpuProcessMonitor<GpuProcessEvent>, GpuMonitorOutputParser_ProcessList>();
            _services.AddSingleton<IIdleMonitorBackgroundService, IdleMonitorBackgroundService>();
            _services.AddSingleton<IIdleMonitor, IdleMonitor>();
            _services.AddSingleton<ProcessFactory, ProcessFactory>();
            _services.AddSingleton<INvidiaSmiParser, NvidiaSmiParser>();
            _services.AddSingleton<INvidiaSmiExecutor, NvidiaSmiExecutor>();

            _services.AddSingleton<IGpuMonitor, GpuMonitor>();
            _services.AddSingleton(new HttpClient());

            //_services.AddSingleton<IBackgroundService<GpuMonitoringBackgroundService>, GpuMonitoringBackgroundService>();
            //_services.AddSingleton<IBackgroundService<IdleMonitorBackgroundService>, IdleMonitorBackgroundService>();
            //_services.AddSingleton<IBackgroundService<EventCoordinatorBackgroundService>, EventCoordinatorBackgroundService>();

            _services.AddSingleton<IMinerProcessFactory, MinerProcessFactory>();
            _services.AddSingleton<IApplicationCancellationTokenFactory>(new DefaultCancellationTokenFactory(_cancellationTokenSource.Token));

            _services.AddHostedService<Collier.Host.GpuMonitoringBackgroundService>();
            //_services.AddHostedService<Collier.Host.IdleMonitoringBackgroundService>();
            //_services.AddHostedService<Collier.Host.EventCoordinatorBackgroundService>();

            //TODO why doesnt this approach work?  Make a sample project and post on stack overflow?

            //_services.AddHostedService<BackgroundServiceHelper<GpuMonitoringBackgroundService>>();
            //_services.AddHostedService<BackgroundServiceHelper<IdleMonitorBackgroundService>>();
            //_services.AddHostedService<BackgroundServiceHelper<EventCoordinatorBackgroundService>>();
            /*
             *      //_services.AddHostedService<GpuMonitoringBackgroundService>();
             *      //_services.AddHostedService<IdleMonitorBackgroundService>();
             *      
             *      If we register the _services themselves with the DI container as singletons types
             *      and also as _services, we will not get singleton behavior.  Apparently
             *      we have to delegate control of the sub _services to the parent service and only
             *      add the parent service as a hosted service.  
             *      
             *      https://github.com/dotnet/extensions/issues/553#issuecomment-462892616
             *      
             *      basically says do not depend on the service itself, depend on a different abstraction
             *      this means event Coordinator background service needs to depend on something else for GPU and Idle background service.
             *      
             *      //TODO remove the weirdness of how I'm doing background service injection
             */

            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(_configuration).CreateLogger();
        }

        private void OnShutdown()
        {
            _cancellationTokenSource.Cancel();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Microsoft.Extensions.Hosting.IHostApplicationLifetime lifetime)
        {

            _hostEnv = env;

            var cancellationSource = new CancellationTokenSource();

            lifetime.ApplicationStopping.Register(OnShutdown);

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

