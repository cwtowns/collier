﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.Host;
using System;
using System.Threading;
using System.Threading.Tasks;
using Collier.Mining;
using CollierService.Monitoring.Gpu;

namespace Collier.Monitoring.Gpu
{
    public class GpuMonitoringBackgroundService : IBackgroundService<GpuMonitoringBackgroundService>, IGpuMonitoringBackgroundService
    {
        public class Settings
        {
            public int PollingIntervalInSeconds { get; set; }
        }

        public event EventHandler<GpuProcessEvent> ProcessEventTriggered;

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

            _logger.LogDebug("settings:  {pollInternal} ", _settings.PollingIntervalInSeconds);
            _logger.LogDebug("GpuMonitoringBackgroundService Created");

            _smiProcessor.GpuActivityNoticed += CheckActivity;
        }

        public void CheckActivity(object o, GpuProcessEvent e)
        {
            ProcessEventTriggered?.Invoke(o, e);

            if (e.ActiveProcesses.Count > 0)
                _miner.Stop();
            else
                _miner.Start();
        }

        public virtual async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("GpuMonitoringBackgroundService is starting.");

            stoppingToken.Register(() =>
                _logger.LogInformation("GpuMonitoringBackgroundService is stopping."));

            //it feels a little weird that this is here, as i don't know that starting the miner is this object's responsibilty.
            //but one nice thing about it being here is I can test for it in a way that makes sense to me, as opposed to some
            //generic BackgroundService object that does this work
            _miner.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                await DoTaskWork();
                await Task.Delay(_settings.PollingIntervalInSeconds * 1000, stoppingToken);
            }

            _logger.LogInformation("GpuMonitoringBackgroundService is stopping.");
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
                _logger.LogError(e, "uncaught exception in executeAsync loop");
            }
        }
    }
}
