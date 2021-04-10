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
using Collier.Mining.OutputParsing;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CollierServiceCollectionExtensions
    {
        public const string ENV_VARIABLE_COLLIER_ROOT_DIRECTORY = "COLLIER_ROOT_DIRECTORY";

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

            services.AddSingleton<IMiner, TrexMiner>();
            services.AddSingleton<IGpuMonitorOutputParser, GpuMonitorOutputParser_ProcessList>();
            services.AddSingleton<ITrexWebClient, TrexWebClient>();
            services.AddSingleton<IBackgroundService<Collier.Monitoring.Gpu.GpuMonitoringBackgroundService>, Collier.Monitoring.Gpu.GpuMonitoringBackgroundService>();
            services.AddSingleton<IGpuProcessMonitor<GpuProcessEvent>, GpuMonitorOutputParser_ProcessList>();

            services.AddSingleton<ProcessFactory, ProcessFactory>();
            services.AddSingleton<INvidiaSmiParser, NvidiaSmiParser>();
            services.AddSingleton<INvidiaSmiExecutor, NvidiaSmiExecutor>();
            services.AddSingleton<IGpuMonitor, GpuMonitor>();
            services.AddSingleton(new HttpClient());


            services.AddSingleton<IMinerProcessFactory, MinerProcessFactory>();
            services.AddSingleton<IApplicationCancellationTokenFactory>(new DefaultCancellationTokenFactory(cancelTokenSource));

            services.AddSingleton<IInternalLoggingFrameworkObserver, InternalLoggingFrameworkObserver>();

            services.AddSingleton<IMiningInfoNotifier, ExternalLoggingFrameworkObserver>();
            services.AddSingleton<IMiningInfoNotifier, HashRateLogObserver>();
            services.AddSingleton<IMiningInfoNotifier, PowerLogObserver>();
            services.AddSingleton<IMiningInfoNotifier, CrashCountLogObserver>();
            services.AddSingleton<IMiningInfoNotifier, TempLogObserver>();

            services.AddSingleton<IMiningInfoNotifier>(x => x.GetRequiredService<TrexMiner>().StateHandler);

            services.AddSingleton<IMinerLogListener, MinerListener>();

            services.AddHostedService<Collier.Host.GpuMonitoringBackgroundService>();
            services.AddHostedService<SignalRBackgroundService>();

            return services;
        }
    }
}
