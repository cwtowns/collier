using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiningAutomater.Host;
using MiningAutomater.Mining;
using MiningAutomater.Monitoring.Gpu;
using MiningAutomater.Monitoring.Idle;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiningAutomater.Monitoring
{
    public class EventCoordinatorBackgroundService : IEventCoordinatorBackgroundService, IBackgroundService<EventCoordinatorBackgroundService>
    {
        public class Settings
        {
            public int PersistedGpuIdleTimeInSeconds { set; get; }
            public int UserIdleTimeInSeconds { set; get; }
            public int OverallPollingIntervalInSeconds { get; set; }
        }

        private readonly ILogger<EventCoordinatorBackgroundService> _logger;
        private readonly IIdleMonitorBackgroundService _idleMonitorService;
        private readonly IGpuMonitoringBackgroundService _gpuMonitorService;
        private readonly IMiner _miner;
        private readonly Settings _settings;

        private IdleEvent _userIdleEvent;
        private GpuIdleEvent _gpuIdleEvent;


        public EventCoordinatorBackgroundService(ILogger<EventCoordinatorBackgroundService> logger, IOptions<Settings> settings, IIdleMonitorBackgroundService idleMonitorService, IGpuMonitoringBackgroundService gpuMonitorService, IMiner miner)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _idleMonitorService = idleMonitorService ?? throw new ArgumentNullException(nameof(idleMonitorService));
            _gpuMonitorService = gpuMonitorService ?? throw new ArgumentNullException(nameof(gpuMonitorService));
            _miner = miner ?? throw new ArgumentNullException(nameof(miner));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings.Value));

            _idleMonitorService.IdleThresholdReached += IdleEventReceived;
            _gpuMonitorService.IdleThresholdReached += GpuEventReceived;
            logger.LogDebug("EventCoordinatorBackgroundService Created");
        }

        public void IdleEventReceived(object o, IdleEvent e)
        {
            _logger.LogDebug("userIdleEventReceived {isIdle}", e.IsIdle);
            //if the last event was idle and this event is also idle, do nothing and consider the user still idle
            if (e.IsIdle && _userIdleEvent != null && _userIdleEvent.IsIdle)
                return;

            //otherwise record the idle event as the last idle event.  t
            _userIdleEvent = e;
        }

        public void GpuEventReceived(object o, GpuIdleEvent e)
        {
            _logger.LogDebug("gpuEventReceived {isIdle}", e.IsIdle);

            //if the last event was idle and this event is also idle, do nothing and consider the user still idle
            if (e.IsIdle && _gpuIdleEvent != null && _gpuIdleEvent.IsIdle)
                return;

            //otherwise record the idle event as the last idle event.  t
            _gpuIdleEvent = e;
        }

        public virtual bool IsSystemIdle()
        {
            if (_gpuIdleEvent == null || _gpuIdleEvent.IsIdle == false)
            {
                _logger.LogDebug("IsSystemIdle {result} {reason}", false, "gpuIsUnderLoad");
                return false;
            }

            if (_userIdleEvent == null || _userIdleEvent.IsIdle == false)
            {
                _logger.LogDebug("IsSystemIdle {result} {reason}", false, "userIsActive");
                return false;
            }

            if (_gpuIdleEvent.TimeSince.TotalSeconds <= _settings.PersistedGpuIdleTimeInSeconds)
            {
                _logger.LogDebug("IsSystemIdle {result} {reason} {timeRemaining}", false, "gpuIdleButThresholdNotReached",
                    (_settings.PersistedGpuIdleTimeInSeconds - _gpuIdleEvent.TimeSince.TotalSeconds));
                return false;
            }

            if (_userIdleEvent.TimeSince.TotalSeconds <= _settings.UserIdleTimeInSeconds)
            {
                _logger.LogDebug("IsSystemIdle {result} {reason} {timeRemaining}", false, "userIsIdleButThresholdNotReached",
                    (_settings.UserIdleTimeInSeconds - _userIdleEvent.TimeSince.TotalSeconds));
                return false;
            }

            _logger.LogDebug("IsSystemIdle {result} {reason}", true, "allConditionsMet");
            return true;
        }

        public virtual async void CheckForSystemIdle()
        {
            if (IsSystemIdle())
            {
                _logger.LogDebug("Overall system is idle");
                if (!await _miner.IsRunningAsync())
                {
                    _logger.LogInformation("Starting miner");
                    _miner.Start();
                }
            }
        }
        public virtual async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EventCoordinatorBackgroundService is starting.");


            stoppingToken.Register(() =>
                _logger.LogInformation("EventCoordinatorBackgroundService is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    CheckForSystemIdle();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "uncaught exception in executeAsync loop");
                }

                await Task.Delay(_settings.OverallPollingIntervalInSeconds * 1000, stoppingToken);
            }

            _logger.LogInformation("EventCoordinatorBackgroundService is stopping.");
        }
    }
}
