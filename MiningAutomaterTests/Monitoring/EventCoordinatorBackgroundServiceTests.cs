using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiningAutomater.Mining;
using MiningAutomater.Monitoring;
using MiningAutomater.Monitoring.Gpu;
using MiningAutomater.Monitoring.Idle;
using Moq;
using System;
using Xunit;

namespace MiningAutomaterTests.Monitoring
{
    //TODO tests around pause and unpause are not present yet because the unset pause URL setting bug was not caught until runtime

    public class EventCoordinatorBackgroundServiceTests
    {
        [Fact]
        public void MiningDoesNotStartWhenNoEventsHaveBeenSeen()
        {
            var logger = new Mock<ILogger<EventCoordinatorBackgroundService>>().Object;
            var settings = new EventCoordinatorBackgroundService.Settings();
            var mockGpuService = new Mock<IGpuMonitoringBackgroundService>();
            var mockIdleService = new Mock<IIdleMonitorBackgroundService>();
            var mockMiner = new Mock<IMiner>();

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object);

            service.CheckForSystemIdle();

            mockMiner.Verify(x => x.Start(), Times.Never, "mining should not start when events have not been seen");
        }

        [Fact]
        public void IdleSystemStartsMinerWhenNotRunning()
        {
            var logger = new Mock<ILogger<EventCoordinatorBackgroundService>>().Object;
            var settings = new EventCoordinatorBackgroundService.Settings();
            var mockGpuService = new Mock<IGpuMonitoringBackgroundService>();
            var mockIdleService = new Mock<IIdleMonitorBackgroundService>();
            var mockMiner = new Mock<IMiner>();
            mockMiner.Setup(x => x.IsRunningAsync()).ReturnsAsync(false);

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object);

            service.GpuEventReceived(mockGpuService.Object, new GpuIdleEvent(DateTime.Now, true));
            service.IdleEventReceived(mockGpuService.Object, new IdleEvent(DateTime.Now, true));

            service.CheckForSystemIdle();

            mockMiner.Verify(x => x.Start(), Times.Once, "mining should attempt to start when idle and miner is not running.");
        }

        [Fact]
        public void IdleSystemRespectsThresholdSettingsForGpu()
        {
            var logger = new Mock<ILogger<EventCoordinatorBackgroundService>>().Object;
            var settings = new EventCoordinatorBackgroundService.Settings();
            settings.PersistedGpuIdleTimeInSeconds = 1000;

            var mockGpuService = new Mock<IGpuMonitoringBackgroundService>();
            var mockIdleService = new Mock<IIdleMonitorBackgroundService>();
            var mockMiner = new Mock<IMiner>();
            mockMiner.Setup(x => x.IsRunningAsync()).ReturnsAsync(false);

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object);

            service.GpuEventReceived(mockGpuService.Object, new GpuIdleEvent(DateTime.Now, true));
            service.IdleEventReceived(mockGpuService.Object, new IdleEvent(DateTime.Now, true));

            service.CheckForSystemIdle();

            mockMiner.Verify(x => x.Start(), Times.Never, "mining should not attempt to start since Gpu is not over idle threshold");
        }

        [Fact]
        public void IdleSystemRespectsThresholdSettingsForUserIdle()
        {
            var logger = new Mock<ILogger<EventCoordinatorBackgroundService>>().Object;
            var settings = new EventCoordinatorBackgroundService.Settings();
            settings.UserIdleTimeInSeconds = 1000;

            var mockGpuService = new Mock<IGpuMonitoringBackgroundService>();
            var mockIdleService = new Mock<IIdleMonitorBackgroundService>();
            var mockMiner = new Mock<IMiner>();
            mockMiner.Setup(x => x.IsRunningAsync()).ReturnsAsync(false);

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object);

            service.GpuEventReceived(mockGpuService.Object, new GpuIdleEvent(DateTime.Now, true));
            service.IdleEventReceived(mockGpuService.Object, new IdleEvent(DateTime.Now, true));

            service.CheckForSystemIdle();

            mockMiner.Verify(x => x.Start(), Times.Never, "mining should not attempt to start since userIdle is not over idle threshold");
        }

        [Fact]
        public void IdleSystemStartsMiningWhenAllThresholdsAreReached()
        {
            var logger = new Mock<ILogger<EventCoordinatorBackgroundService>>().Object;
            var settings = new EventCoordinatorBackgroundService.Settings();
            settings.UserIdleTimeInSeconds = 1;
            settings.PersistedGpuIdleTimeInSeconds = 1;

            var mockGpuService = new Mock<IGpuMonitoringBackgroundService>();
            var mockIdleService = new Mock<IIdleMonitorBackgroundService>();
            var mockMiner = new Mock<IMiner>();
            mockMiner.Setup(x => x.IsRunningAsync()).ReturnsAsync(false);

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object);

            service.GpuEventReceived(mockGpuService.Object, new GpuIdleEvent(DateTime.Now.AddDays(-3), true));
            service.GpuEventReceived(mockGpuService.Object, new GpuIdleEvent(DateTime.Now.AddDays(-2), true));
            service.GpuEventReceived(mockGpuService.Object, new GpuIdleEvent(DateTime.Now.AddDays(-1), true));
            service.IdleEventReceived(mockGpuService.Object, new IdleEvent(DateTime.Now.AddDays(-3), true));
            service.IdleEventReceived(mockGpuService.Object, new IdleEvent(DateTime.Now.AddDays(-2), true));
            service.IdleEventReceived(mockGpuService.Object, new IdleEvent(DateTime.Now.AddDays(-1), true));

            service.CheckForSystemIdle();

            mockMiner.Verify(x => x.Start(), Times.Once, "mining should start as both events are past the threshold");
        }


        [Fact]
        public void IdleSystemDoesNotStartMinerWhenMinerRunning()
        {
            var logger = new Mock<ILogger<EventCoordinatorBackgroundService>>().Object;
            var settings = new EventCoordinatorBackgroundService.Settings();
            var mockGpuService = new Mock<IGpuMonitoringBackgroundService>();
            var mockIdleService = new Mock<IIdleMonitorBackgroundService>();
            var mockMiner = new Mock<IMiner>();
            mockMiner.Setup(x => x.IsRunningAsync()).ReturnsAsync(true);

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object);

            service.GpuEventReceived(mockGpuService.Object, new GpuIdleEvent(DateTime.Now, true));
            service.IdleEventReceived(mockGpuService.Object, new IdleEvent(DateTime.Now, true));

            service.CheckForSystemIdle();

            mockMiner.Verify(x => x.Start(), Times.Never, "mining is already running so it should not attempt to start again");
        }

        [Fact]
        public void GpuEventOnlyIsNotEnoughToStartMining()
        {
            var logger = new Mock<ILogger<EventCoordinatorBackgroundService>>().Object;
            var settings = new EventCoordinatorBackgroundService.Settings();
            var mockGpuService = new Mock<IGpuMonitoringBackgroundService>();
            var mockIdleService = new Mock<IIdleMonitorBackgroundService>();
            var mockMiner = new Mock<IMiner>();
            mockMiner.Setup(x => x.IsRunningAsync()).ReturnsAsync(false);

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object);

            service.GpuEventReceived(mockGpuService.Object, new GpuIdleEvent(DateTime.Now, true));
            service.IdleEventReceived(mockGpuService.Object, new IdleEvent(DateTime.Now, false));

            service.CheckForSystemIdle();

            mockMiner.Verify(x => x.Start(), Times.Never, "both events are needed to trigger a miner start");
        }

        [Fact]
        public void UserIdleEventOnlyIsNotEnoughToStartMining()
        {
            var logger = new Mock<ILogger<EventCoordinatorBackgroundService>>().Object;
            var settings = new EventCoordinatorBackgroundService.Settings();
            var mockGpuService = new Mock<IGpuMonitoringBackgroundService>();
            var mockIdleService = new Mock<IIdleMonitorBackgroundService>();
            var mockMiner = new Mock<IMiner>();
            mockMiner.Setup(x => x.IsRunningAsync()).ReturnsAsync(false);

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object);

            service.GpuEventReceived(mockGpuService.Object, new GpuIdleEvent(DateTime.Now, false));
            service.IdleEventReceived(mockGpuService.Object, new IdleEvent(DateTime.Now, true));

            service.CheckForSystemIdle();

            mockMiner.Verify(x => x.Start(), Times.Never, "both events are needed to trigger a miner start");
        }
    }
}
