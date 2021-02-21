﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.Host;
using System;
using System.Threading;
using System.Threading.Tasks;
using Collier.Mining;

namespace Collier.Monitoring.Gpu
{
    public class GpuMonitoringBackgroundService : IGpuMonitoringBackgroundService, IBackgroundService<GpuMonitoringBackgroundService>
    {
        public class Settings
        {
            public int PollingIntervalInSeconds { get; set; }
        }

        public event EventHandler<GpuIdleEvent> IdleThresholdReached;

        private readonly ILogger<GpuMonitoringBackgroundService> _logger;
        private readonly IGpuMonitor _gpuMonitor;
        private readonly Settings _settings;
        private readonly IMiner _miner;

        public GpuMonitoringBackgroundService(ILogger<GpuMonitoringBackgroundService> logger, IGpuMonitor gpuMonitor, IOptions<Settings> settings, IMiner miner)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gpuMonitor = gpuMonitor ?? throw new ArgumentNullException(nameof(gpuMonitor));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings.Value));
            _miner = miner ?? throw new ArgumentNullException(nameof(miner));

            _logger.LogDebug("settings:  {pollInternal} ", _settings.PollingIntervalInSeconds);
            logger.LogDebug("GpuMonitoringBackgroundService Created");

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
                //this should publish an event indicating we're idle
                //the thing that subscribes to that event should monitor for how long we wait to be consdered GPU idle
                //another thing is monitoring the user too, and publishing tht event
                //there is a subscriber that looks for both events to be true and then starts mining

                var isUnderLoad = await _gpuMonitor.IsGpuUnderLoadAsync();

                _logger.LogDebug("gpuIsUnderLoad:  {gpuIsUnderLoad}", isUnderLoad);
                //TODO idlethreshold is the wrong name now because we're not talking about it being idle

                IdleThresholdReached?.Invoke(this, new GpuIdleEvent(DateTime.Now, !isUnderLoad));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "uncaught exception in executeAsync loop");
            }
        }
    }
}
