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

namespace CollierTests.Mining
{
    public class HashRateLogObserverTests
    {
        [Fact]
        public void HashRateBelowCapacityTest()
        {
            var mockLogger = new Mock<ILogger<HashRateLogObserver>>();
            var mockListener = new Mock<IMinerLogListener>();
            var options = new HashRateLogObserver.Settings() { StatHistorySize = 2 };

            var objectUnderTest =
                new HashRateLogObserver(mockLogger.Object, Options.Create(options), mockListener.Object);

            const double rate1 = 86.01d;
            const double rate2 = 24.01d;
            var mean = (rate1 + rate2) / 2.0;

            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - " + rate1 + " MH/s, [T:66C, P:253W, F:84%, E:341kH/W], 335/335 R:0%" });
            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - " + rate2 + " MH/s, [T:66C, P:253W, F:84%, E:341kH/W], 335/335 R:0%" });

            objectUnderTest.AverageHashRate.Should().Be(mean, "this is how you calculate the mean");
        }

        [Fact]
        public void HashRateAboveCapacityTest()
        {
            var mockLogger = new Mock<ILogger<HashRateLogObserver>>();
            var mockListener = new Mock<IMinerLogListener>();
            var options = new HashRateLogObserver.Settings() { StatHistorySize = 2 };

            var objectUnderTest =
                new HashRateLogObserver(mockLogger.Object, Options.Create(options), mockListener.Object);

            const double rate1 = 86.01d;
            const double rate2 = 24.01d;
            const double rate3 = 334.01d;
            var mean = (rate3 + rate2) / 2.0;

            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - " + rate1 + " MH/s, [T:66C, P:253W, F:84%, E:341kH/W], 335/335 R:0%" });
            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - " + rate2 + " MH/s, [T:66C, P:253W, F:84%, E:341kH/W], 335/335 R:0%" });
            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - " + rate3 + " MH/s, [T:66C, P:253W, F:84%, E:341kH/W], 335/335 R:0%" });

            objectUnderTest.AverageHashRate.Should().Be(mean, "the first hash rate should be pushed out of the queue");
            objectUnderTest.LastHashRate.Should().Be(rate3, "This was the last rate we logged.");
        }

        [Fact]
        public void HashRateNoEntryIsZero()
        {
            var mockLogger = new Mock<ILogger<HashRateLogObserver>>();
            var mockListener = new Mock<IMinerLogListener>();
            var options = new HashRateLogObserver.Settings() { StatHistorySize = 2 };

            var objectUnderTest =
                new HashRateLogObserver(mockLogger.Object, Options.Create(options), mockListener.Object);

            objectUnderTest.AverageHashRate.Should().Be(0, "we have no data");
        }

        [Fact]
        public void TempEventsAreBroadcast()
        {
            var mockLogger = new Mock<ILogger<HashRateLogObserver>>();
            var mockListener = new Mock<IMinerLogListener>();
            var options = new HashRateLogObserver.Settings() { StatHistorySize = 2 };

            var objectUnderTest =
                new HashRateLogObserver(mockLogger.Object, Options.Create(options), mockListener.Object);

            var receivedAverage = false;
            var receivedLast = false;

            const double rate1 = 86.01d;

            objectUnderTest.MiningInformationChanged += ((sender, info) =>
            {
                if (info.Name.Equals("AverageHashRate"))
                    receivedAverage = true;
                if (info.Name.Equals("LastHashRate"))
                    receivedLast = true;

                info.Value.Should().Be(rate1.ToString(),
                    "the event value should be equal to the log value since there was only one log message sent.");
            });

            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - " + rate1 + " MH/s, [T:66C, P:253W, F:84%, E:341kH/W], 335/335 R:0%" });

            receivedAverage.Should().Be(true, "we should have broadcast an event.");
            receivedLast.Should().Be(true, "we should have broadcast an event.");
        }
    }
}
