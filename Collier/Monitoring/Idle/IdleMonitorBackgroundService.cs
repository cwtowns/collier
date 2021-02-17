using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiningAutomater.Host;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiningAutomater.Monitoring.Idle
{
    public class IdleMonitorBackgroundService : IIdleMonitorBackgroundService, IBackgroundService<IdleMonitorBackgroundService>
    {
        public class Settings
        {
            public int PollingIntervalInSeconds { get; set; }
        }

        public event EventHandler<IdleEvent> IdleThresholdReached;

        private readonly ILogger<IdleMonitorBackgroundService> _logger;
        private readonly Settings _settings;
        private readonly IIdleMonitor _idleMonitor;

        public IdleMonitorBackgroundService(ILogger<IdleMonitorBackgroundService> logger, IOptions<Settings> settings, IIdleMonitor idleMonitor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings.Value));
            _idleMonitor = idleMonitor ?? throw new ArgumentNullException(nameof(idleMonitor));
            logger.LogDebug("IdleMonitorBackgroundService Created");
        }

        public virtual async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("IdleMonitorBackgroundService is starting.");

            stoppingToken.Register(() =>
                _logger.LogInformation("IdleMonitorBackgroundService background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("IdleMonitorBackgroundService task doing background work.");

                    CheckIdleState();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "uncaught exception in executeAsync loop");
                }

                await Task.Delay(_settings.PollingIntervalInSeconds * 1000, stoppingToken);
            }

            _logger.LogInformation("IdleMonitorBackgroundService is stopping.");
        }

        public virtual void CheckIdleState()
        {
            IdleThresholdReached?.Invoke(this, new IdleEvent(DateTime.Now, _idleMonitor.IsUserIdlingPastThreshold()));
        }
    }
}
