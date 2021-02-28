using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Collier.Host;
using Collier.IO;
using Collier.Mining;
using Collier.Monitoring.Gpu;
using Collier.Monitoring.Idle;
using CollierService.Mining;
using CollierService.Monitoring.Gpu;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CollierServiceCollectionExtensions
    {
        public static string ENV_VARIABLE_COLLIER_ROOT_DIRECTORY = "COLLIER_ROOT_DIRECTORY";

        public static IServiceCollection AddCollier(
            this IServiceCollection services, IConfiguration config, CancellationTokenSource cancelTokenSource)
        {
            var monitoringSection = config.GetSection("monitoring");
            var tRexMinerSection = config.GetSection("miner").GetSection("t-rex");

            services.Configure<TrexMiner.Settings>(options => tRexMinerSection.Bind(options));
            services.Configure<TrexWebClient.Settings>(options => tRexMinerSection.GetSection("web").Bind(options));


            var gpuMonitoring = monitoringSection.GetSection("gpuMonitoring");

            services.Configure<Collier.Monitoring.Gpu.GpuMonitoringBackgroundService.Settings>(options => gpuMonitoring.Bind(options));

            services.Configure<GpuMonitorOutputParser_ProcessList.Settings>(options => gpuMonitoring.GetSection("outputParsers").GetSection("processListOutputParser").Bind(options));

            services.Configure<NvidiaSmiExecutor.Settings>(options => gpuMonitoring.Bind(options));


            services.Configure<IdleMonitor.Settings>(options => monitoringSection.GetSection("userIdle").Bind(options));
            services.Configure<IdleMonitorBackgroundService.Settings>(options => monitoringSection.GetSection("userIdle").Bind(options));



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

            services.AddSingleton<IMiner, TrexMiner>();
            services.AddSingleton<IGpuMonitorOutputParser, GpuMonitorOutputParser_ProcessList>();
            services.AddSingleton<ITrexWebClient, TrexWebClient>();
            services.AddSingleton<IGpuMonitoringBackgroundService, Collier.Monitoring.Gpu.GpuMonitoringBackgroundService>();
            services.AddSingleton<IGpuProcessMonitor<GpuProcessEvent>, GpuMonitorOutputParser_ProcessList>();
            services.AddSingleton<IIdleMonitorBackgroundService, IdleMonitorBackgroundService>();
            services.AddSingleton<IIdleMonitor, IdleMonitor>();
            services.AddSingleton<ProcessFactory, ProcessFactory>();
            services.AddSingleton<INvidiaSmiParser, NvidiaSmiParser>();
            services.AddSingleton<INvidiaSmiExecutor, NvidiaSmiExecutor>();
            services.AddSingleton<ITrexLogModifier, DateStrippingTrexLogModifier>();
            services.AddSingleton<IGpuMonitor, GpuMonitor>();
            services.AddSingleton(new HttpClient());

            services.AddSingleton<IMinerProcessFactory, MinerProcessFactory>();
            services.AddSingleton<IApplicationCancellationTokenFactory>(new DefaultCancellationTokenFactory(cancelTokenSource.Token));

            services.AddHostedService<Collier.Host.GpuMonitoringBackgroundService>();

            return services;
        }
    }
}
