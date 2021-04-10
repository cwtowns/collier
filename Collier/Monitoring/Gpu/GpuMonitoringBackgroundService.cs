using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.Host;
using System;
using System.Threading;
using System.Threading.Tasks;
using Collier.Mining;
using Collier.Monitoring.Gpu;
using Collier.Mining.State;

namespace Collier.Monitoring.Gpu
{
    public class GpuMonitoringBackgroundService : IBackgroundService<GpuMonitoringBackgroundService>
    {
        public class Settings
        {
            public int PollingIntervalInSeconds { get; set; }
        }

        private readonly ILogger<GpuMonitoringBackgroundService> _logger;
        private readonly INvidiaSmiExecutor _smiExecutor;
        private readonly Settings _settings;
        private readonly IMiner _miner;
        private readonly IGpuProcessMonitor<GpuProcessEvent> _smiProcessor;

        public GpuMonitoringBackgroundService(ILogger<GpuMonitoringBackgroundService> logger, IOptions<Settings> settings, IMiner miner, INvidiaSmiExecutor smiExecutor, IGpuProcessMonitor<GpuProcessEvent> smiProcessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _smiExecutor = smiExecutor ?? throw new ArgumentNullException(nameof(smiExecutor));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings.Value));
            _miner = miner ?? throw new ArgumentNullException(nameof(miner));
            _smiProcessor = smiProcessor ?? throw new ArgumentNullException(nameof(smiProcessor));

            _logger.LogInformation("{methodName} {message} {pollInterval}", "Constructor",
                "settings.PollingIntervalInSeconds", _settings.PollingIntervalInSeconds);
            _logger.LogInformation("{methodName} {message}", "Constructor", "GpuMonitoringBackgroundService Created");

            _smiProcessor.GpuActivityNoticed += CheckActivity;
        }

        public async void CheckActivity(object o, GpuProcessEvent e)
        {
            try
            {
                if (e.ActiveProcesses.Count == 0)
                {
                    if (await _miner.TransitionToStateAsync(new MinerStartedFromNoGaming()))
                    {
                        _logger.LogInformation("{methodName} {message}", "CheckActivity",
                            "Starting mining because no processes are running.");
                    }
                }
                else if (e.ActiveProcesses.Count > 0)
                {
                    if (await _miner.TransitionToStateAsync(new MinerStoppedFromGaming()))
                    {
                        _logger.LogInformation("{methodName} {message} {processList}", "CheckActivity",
                            "Stopping mining because the following processes were found",
                            string.Join(',', e.ActiveProcesses));
                    }
                }
            }
            catch (Exception err)
            {
                _logger.LogError(err, "{methodName} {message}", "CheckActivity", "error during async processing");
            }
        }

        public virtual async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{methodName} {message}", "ExecuteAsync", "starting");

            stoppingToken.Register(() =>
                _logger.LogInformation("{methodName} {message}", "ExecuteAsync", "stopping"));

            while (!stoppingToken.IsCancellationRequested)
            {
                await DoTaskWork();
                await Task.Delay(_settings.PollingIntervalInSeconds * 1000, stoppingToken);
            }

            _logger.LogInformation("{methodName} {message}", "ExecuteAsync", "stopping");
        }

        public virtual async Task DoTaskWork()
        {
            try
            {
                var commandOutput = await _smiExecutor.ExecuteCommandAsync();

                if (_smiExecutor.HasErrored)
                    throw new ArgumentOutOfRangeException("unable to exeute smi program.  commandOutput:  " + commandOutput);

                _smiProcessor.CheckGpuOutput(commandOutput);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{methodName} {message}", "ExecuteAsync", "uncaught exception in executeAsync loop");
            }
        }
    }
}
