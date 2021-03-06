using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.Host;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Collier.Monitoring.Idle
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
            _logger.LogDebug("IdleMonitorBackgroundService Created");
        }

        public virtual async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("IdleMonitorBackgroundService is starting.");

            stoppingToken.Register(() =>
                _logger.LogInformation("IdleMonitorBackgroundService background task is stopping."));

            do
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

                if (stoppingToken.IsCancellationRequested)
                    break;
                await Task.Delay(_settings.PollingIntervalInSeconds * 1000, stoppingToken);

            } while (!stoppingToken.IsCancellationRequested);

            _logger.LogInformation("IdleMonitorBackgroundService is stopping.");
        }

        public virtual void CheckIdleState()
        {
            IdleThresholdReached?.Invoke(this, new IdleEvent(DateTime.Now, _idleMonitor.IsUserIdlingPastThreshold()));
        }
    }
}
