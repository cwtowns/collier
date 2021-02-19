using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.Monitoring.Gpu;
using Moq;
using System;
using System.Threading;
using Collier.Mining;
using Xunit;

namespace CollierTests.Monitoring
{
    public class GpuMonitorBackgroundServiceTests
    {
        [Fact]
        public async void StartingTheServiceStartsTheMiner()
        {
            var monitor = new Mock<IGpuMonitor>();
            monitor.Setup(x => x.IsGpuUnderLoadAsync()).ReturnsAsync(false);
            var minerMock = new Mock<IMiner>();

            var settings = new GpuMonitoringBackgroundService.Settings() { PollingIntervalInSeconds = 1 };

            var backgroundService = new GpuMonitoringBackgroundService(
                new Mock<ILogger<GpuMonitoringBackgroundService>>().Object,
                monitor.Object,
                Options.Create(settings), minerMock.Object);

            var source = new CancellationTokenSource();
            source.Cancel();
            await backgroundService.ExecuteAsync(source.Token);


            minerMock.Verify(x => x.Start(), Times.AtLeastOnce, "miner should start when the background monitoring service starts to avoid rogue background processes.");
        }

        [Fact]
        public async void NotificationSentWhenIdleDetected()
        {
            var monitor = new Mock<IGpuMonitor>();
            monitor.Setup(x => x.IsGpuUnderLoadAsync()).ReturnsAsync(false);
            var minerMock = new Mock<IMiner>();

            var settings = new GpuMonitoringBackgroundService.Settings() { PollingIntervalInSeconds = 1 };

            var backgroundService = new GpuMonitoringBackgroundService(
                new Mock<ILogger<GpuMonitoringBackgroundService>>().Object,
                monitor.Object,
                Options.Create(settings), minerMock.Object);

            GpuIdleEvent capturedEvent = null;
            var action = new Action<object, GpuIdleEvent>((o, e) => { capturedEvent = e; });
            var eventHandlerDelegate = new System.EventHandler<GpuIdleEvent>(action);
            backgroundService.IdleThresholdReached += eventHandlerDelegate;

            await backgroundService.DoTaskWork();

            capturedEvent.IsIdle.Should().BeTrue("because when the user has moved to the idle state we should send the event.");
        }

        [Fact]
        public async void NoNotificationSentWhengpuInUse()
        {
            var monitor = new Mock<IGpuMonitor>();
            var minerMock = new Mock<IMiner>();
            monitor.Setup(x => x.IsGpuUnderLoadAsync()).ReturnsAsync(true);

            var settings = new GpuMonitoringBackgroundService.Settings() { PollingIntervalInSeconds = 1 };

            var backgroundService = new GpuMonitoringBackgroundService(
                new Mock<ILogger<GpuMonitoringBackgroundService>>().Object,
                monitor.Object,
                Options.Create(settings),
                minerMock.Object);

            GpuIdleEvent capturedEvent = null;
            var action = new Action<object, GpuIdleEvent>((o, e) => { capturedEvent = e; });
            var eventHandlerDelegate = new System.EventHandler<GpuIdleEvent>(action);
            backgroundService.IdleThresholdReached += eventHandlerDelegate;

            await backgroundService.DoTaskWork();

            capturedEvent.IsIdle.Should().BeFalse("because when under load no notification should be sent.");
        }
    }


}
