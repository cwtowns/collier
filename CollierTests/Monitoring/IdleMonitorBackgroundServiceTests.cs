using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.Monitoring.Idle;
using Moq;
using System;
using Xunit;

namespace CollierTests.Monitoring
{
    public class IdleMonitorBackgroundServiceTests
    {
        [Fact]
        public void NotificationSentWhenIdleDetected()
        {
            var monitor = new Mock<IIdleMonitor>();
            monitor.Setup(x => x.IsUserIdlingPastThreshold()).Returns(true);

            var settings = new IdleMonitorBackgroundService.Settings() { PollingIntervalInSeconds = 1 };

            var backgroundService = new IdleMonitorBackgroundService(
                new Mock<ILogger<IdleMonitorBackgroundService>>().Object,
                Options.Create(settings),
                monitor.Object);

            IdleEvent capturedEvent = null;
            var action = new Action<object, IdleEvent>((o, e) => { capturedEvent = e; });
            var eventHandlerDelegate = new System.EventHandler<IdleEvent>(action);
            backgroundService.IdleThresholdReached += eventHandlerDelegate;

            backgroundService.CheckIdleState();

            capturedEvent.IsIdle.Should().BeTrue("because when the user has moved to the idle state we should send the event.");
        }

        [Fact]
        public void NoNotificationSentWhenIdleDetected()
        {
            var monitor = new Mock<IIdleMonitor>();
            monitor.Setup(x => x.IsUserIdlingPastThreshold()).Returns(false);

            var settings = new IdleMonitorBackgroundService.Settings() { PollingIntervalInSeconds = 1 };

            var backgroundService = new IdleMonitorBackgroundService(
                new Mock<ILogger<IdleMonitorBackgroundService>>().Object,
                Options.Create(settings),
                monitor.Object);

            IdleEvent capturedEvent = null;
            var action = new Action<object, IdleEvent>((o, e) => { capturedEvent = e; });
            var eventHandlerDelegate = new System.EventHandler<IdleEvent>(action);
            backgroundService.IdleThresholdReached += eventHandlerDelegate;

            backgroundService.CheckIdleState();

            capturedEvent.IsIdle.Should().BeFalse("because when is not idle no notification should be sent.");
        }
    }
}
