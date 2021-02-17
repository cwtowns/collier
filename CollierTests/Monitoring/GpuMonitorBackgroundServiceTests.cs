using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.Monitoring.Gpu;
using Moq;
using System;
using Xunit;

namespace CollierTests.Monitoring
{
    public class GpuMonitorBackgroundServiceTests
    {
        [Fact]
        public async void NotificationSentWhenIdleDetected()
        {
            var monitor = new Mock<IGpuMonitor>();
            monitor.Setup(x => x.IsGpuUnderLoadAsync()).ReturnsAsync(false);

            var settings = new GpuMonitoringBackgroundService.Settings() { PollingIntervalInSeconds = 1 };

            var backgroundService = new GpuMonitoringBackgroundService(
                new Mock<ILogger<GpuMonitoringBackgroundService>>().Object,
                monitor.Object,
                Options.Create(settings));

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
            monitor.Setup(x => x.IsGpuUnderLoadAsync()).ReturnsAsync(true);

            var settings = new GpuMonitoringBackgroundService.Settings() { PollingIntervalInSeconds = 1 };

            var backgroundService = new GpuMonitoringBackgroundService(
                new Mock<ILogger<GpuMonitoringBackgroundService>>().Object,
                monitor.Object,
                Options.Create(settings));

            GpuIdleEvent capturedEvent = null;
            var action = new Action<object, GpuIdleEvent>((o, e) => { capturedEvent = e; });
            var eventHandlerDelegate = new System.EventHandler<GpuIdleEvent>(action);
            backgroundService.IdleThresholdReached += eventHandlerDelegate;

            await backgroundService.DoTaskWork();

            capturedEvent.IsIdle.Should().BeFalse("because when under load no notification should be sent.");
        }
    }


}
