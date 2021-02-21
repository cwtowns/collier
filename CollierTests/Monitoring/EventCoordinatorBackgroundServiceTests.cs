using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.Mining;
using Collier.Monitoring;
using Collier.Monitoring.Gpu;
using Collier.Monitoring.Idle;
using Moq;
using System;
using Xunit;

namespace CollierTests.Monitoring
{
    public class EventCoordinatorBackgroundServiceTests
    {
        [Fact]
        public void MiningDoesNotStartWhenNoEventsHaveBeenSeen()
        {
            var logger = new Mock<ILogger<EventCoordinatorBackgroundService>>().Object;
            var settings = new EventCoordinatorBackgroundService.Settings();
            var mockGpuService = new Mock<IGpuMonitoringBackgroundService>();
            var mockIdleService = new Mock<IIdleMonitorBackgroundService>();
            var mockProcesservice = new Mock<IGpuMonitoringBackgroundService2>();
            var mockMiner = new Mock<IMiner>();

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object, mockProcesservice.Object);

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
            var mockProcesservice = new Mock<IGpuMonitoringBackgroundService2>();
            var mockMiner = new Mock<IMiner>();
            mockMiner.Setup(x => x.IsRunningAsync()).ReturnsAsync(false);

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object, mockProcesservice.Object);

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
            var mockProcesservice = new Mock<IGpuMonitoringBackgroundService2>();
            var mockMiner = new Mock<IMiner>();
            mockMiner.Setup(x => x.IsRunningAsync()).ReturnsAsync(false);

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object, mockProcesservice.Object);

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
            var mockProcesservice = new Mock<IGpuMonitoringBackgroundService2>();
            var mockMiner = new Mock<IMiner>();
            mockMiner.Setup(x => x.IsRunningAsync()).ReturnsAsync(false);

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object, mockProcesservice.Object);

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
            var mockProcesservice = new Mock<IGpuMonitoringBackgroundService2>();
            var mockIdleService = new Mock<IIdleMonitorBackgroundService>();
            var mockMiner = new Mock<IMiner>();
            mockMiner.Setup(x => x.IsRunningAsync()).ReturnsAsync(false);

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object, mockProcesservice.Object);

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
        public void IdleSystemAttemptsToStartMiner()
        {
            var logger = new Mock<ILogger<EventCoordinatorBackgroundService>>().Object;
            var settings = new EventCoordinatorBackgroundService.Settings();
            var mockGpuService = new Mock<IGpuMonitoringBackgroundService>();
            var mockIdleService = new Mock<IIdleMonitorBackgroundService>();
            var mockProcesservice = new Mock<IGpuMonitoringBackgroundService2>();
            var mockMiner = new Mock<IMiner>();
            mockMiner.Setup(x => x.IsRunningAsync()).ReturnsAsync(true);

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object, mockProcesservice.Object);

            service.GpuEventReceived(mockGpuService.Object, new GpuIdleEvent(DateTime.Now, true));
            service.IdleEventReceived(mockGpuService.Object, new IdleEvent(DateTime.Now, true));

            service.CheckForSystemIdle();

            mockMiner.Verify(x => x.Start(), Times.Once, "idle events should start the miner");
        }

        [Fact]
        public void GpuEventOnlyIsNotEnoughToStartMining()
        {
            var logger = new Mock<ILogger<EventCoordinatorBackgroundService>>().Object;
            var settings = new EventCoordinatorBackgroundService.Settings();
            var mockGpuService = new Mock<IGpuMonitoringBackgroundService>();
            var mockProcesservice = new Mock<IGpuMonitoringBackgroundService2>();
            var mockIdleService = new Mock<IIdleMonitorBackgroundService>();
            var mockMiner = new Mock<IMiner>();
            mockMiner.Setup(x => x.IsRunningAsync()).ReturnsAsync(false);

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object, mockProcesservice.Object);

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
            var mockProcesservice = new Mock<IGpuMonitoringBackgroundService2>();
            var mockMiner = new Mock<IMiner>();
            mockMiner.Setup(x => x.IsRunningAsync()).ReturnsAsync(false);

            var service = new EventCoordinatorBackgroundService(logger, Options.Create(settings), mockIdleService.Object, mockGpuService.Object, mockMiner.Object, mockProcesservice.Object);

            service.GpuEventReceived(mockGpuService.Object, new GpuIdleEvent(DateTime.Now, false));
            service.IdleEventReceived(mockGpuService.Object, new IdleEvent(DateTime.Now, true));

            service.CheckForSystemIdle();

            mockMiner.Verify(x => x.Start(), Times.Never, "both events are needed to trigger a miner start");
        }
    }
}
