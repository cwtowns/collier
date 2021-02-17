using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.Monitoring.Idle;
using Moq;
using System;
using Xunit;

namespace CollierTests.Monitoring
{
    public class IdleMonitorTests
    {
        [Fact]
        public void IdlingPastThresholdWorks()
        {
            var settings = Options.Create(new IdleMonitor.Settings());
            var monitor = new Mock<IdleMonitor>(new Mock<ILogger<IdleMonitor>>().Object, settings);
            monitor.CallBase = true;
            monitor.Setup(x => x.Threshold).Returns(TimeSpan.FromTicks(4));
            monitor.Setup(x => x.IdleTime).Returns(TimeSpan.FromTicks(5));

            monitor.Object.IsUserIdlingPastThreshold().Should().BeTrue("because the last input is greater than the threshold.");
        }

        [Fact]
        public void IdlingBeforeThresholdWorks()
        {
            var settings = Options.Create(new IdleMonitor.Settings());
            var monitor = new Mock<IdleMonitor>(new Mock<ILogger<IdleMonitor>>().Object, settings);
            monitor.CallBase = true;
            monitor.Setup(x => x.Threshold).Returns(TimeSpan.FromTicks(5));
            monitor.Setup(x => x.IdleTime).Returns(TimeSpan.FromTicks(4));

            monitor.Object.IsUserIdlingPastThreshold().Should().BeFalse("because the last input is less than the threshold.");
        }
    }

}
