using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collier.Mining;
using Collier.Mining.Trex.OutputParsing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;
using FluentAssertions;
using Collier.Mining.Trex;

namespace CollierTests.Mining
{
    public class MinerUpdateObserverTests
    {
        [Fact]
        public void ObserverNotifiesWhenUpdateAvailable()
        {
            var mockLogger = new Mock<ILogger<MinerUpdateAvailableObserver>>();
            var mockListener = new Mock<IMinerLogListener>();

            var objectUnderTest =
                new MinerUpdateAvailableObserver(mockLogger.Object, mockListener.Object);

            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "stuff New version of T-Rex is available" });

            objectUnderTest.HasUpdate.Should().Be(true, "we indicated an update was avialable");
        }

        [Fact]
        public void ObserverDoesNotNotifyWhenNoUpdateAvailable()
        {
            var mockLogger = new Mock<ILogger<MinerUpdateAvailableObserver>>();
            var mockListener = new Mock<IMinerLogListener>();

            var objectUnderTest =
                new MinerUpdateAvailableObserver(mockLogger.Object, mockListener.Object);

            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "stuff" });

            objectUnderTest.HasUpdate.Should().Be(false, "no update is available");
        }

        [Fact]
        public void ObserverDoesNotNotifyAfterRestart()
        {
            var mockLogger = new Mock<ILogger<MinerUpdateAvailableObserver>>();
            var mockListener = new Mock<IMinerLogListener>();

            var objectUnderTest =
                new MinerUpdateAvailableObserver(mockLogger.Object, mockListener.Object);

            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "stuff New version of T-Rex is available" });
            objectUnderTest.HasUpdate.Should().Be(true, "we indicated an update was avialable");
            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = TrexMiner.STARTUP_LOG_MESSAGE });

            objectUnderTest.HasUpdate.Should().Be(false, "the miner has restarted so no update should be available");
        }
    }
}

