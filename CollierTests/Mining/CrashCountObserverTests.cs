using Collier.Mining;
using Collier.Mining.OutputParsing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;
using FluentAssertions;


namespace CollierTests.Mining
{
    public class CrashCountObserverTests
    {
        [Fact]
        public void CrashCountIsCounted()
        {
            var mockLogger = new Mock<ILogger<CrashCountLogObserver>>();
            var mockListener = new Mock<IMinerLogListener>();

            var objectUnderTest =
                new CrashCountLogObserver(mockLogger.Object, mockListener.Object);

            const int crashCount = 3;

            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU CRASH LIST" });
            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "WD: GPU#0: " + crashCount });
            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "WD: GPU#0: " + crashCount });

            objectUnderTest.CrashCount.Should().Be(crashCount, "this is how many times we said we crashed");
        }

        [Fact]
        public void CrashCountWithoutPrefixIsNotCounted()
        {
            var mockLogger = new Mock<ILogger<CrashCountLogObserver>>();
            var mockListener = new Mock<IMinerLogListener>();

            var objectUnderTest =
                new CrashCountLogObserver(mockLogger.Object, mockListener.Object);

            const int crashCount = 3;
            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "WD: GPU#0: " + crashCount });

            objectUnderTest.CrashCount.Should().Be(0, "the crash count prefix identifier was not sent");
        }

        [Fact]
        public void CrashCountIsResetOnRestart()
        {
            var mockLogger = new Mock<ILogger<CrashCountLogObserver>>();
            var mockListener = new Mock<IMinerLogListener>();

            var objectUnderTest =
                new CrashCountLogObserver(mockLogger.Object, mockListener.Object);

            const int crashCount = 3;

            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU CRASH LIST" });
            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "WD: GPU#0: " + crashCount });
            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "ApiServer: Telnet server started on" });

            objectUnderTest.CrashCount.Should().Be(0, "we faked restarting the miner");
        }
    }
}
