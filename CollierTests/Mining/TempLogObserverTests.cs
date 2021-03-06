using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collier.Mining;
using Collier.Mining.OutputParsing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;
using FluentAssertions;

namespace CollierTests.Mining
{
    public class TempLogObserverTests
    {
        [Fact]
        public void TempBelowCapacityTest()
        {
            var mockLogger = new Mock<ILogger<TempLogObserver>>();
            var mockListener = new Mock<IMinerLogListener>();
            var options = new TempLogObserver.Settings() { StatHistorySize = 2 };

            var objectUnderTest =
                new TempLogObserver(mockLogger.Object, Options.Create(options), mockListener.Object);

            const int temp1 = 100;
            const int temp2 = 202;
            var mean = (temp1 + temp2) / 2;

            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - 85.99 MH/s, [T:" + temp1 + "C, P:252W, F:81%, E:341kH/W], 337/337 R:0%" });
            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - 85.99 MH/s, [T:" + temp2 + "C, P:252W, F:81%, E:341kH/W], 337/337 R:0%" });

            objectUnderTest.AverageTemp.Should().Be(mean, "this is how you calculate the mean");
        }

        [Fact]
        public void TempAboveCapacityTest()
        {
            var mockLogger = new Mock<ILogger<TempLogObserver>>();
            var mockListener = new Mock<IMinerLogListener>();
            var options = new TempLogObserver.Settings() { StatHistorySize = 2 };

            var objectUnderTest =
                new TempLogObserver(mockLogger.Object, Options.Create(options), mockListener.Object);

            const int temp1 = 100;
            const int temp2 = 202;
            const int temp3 = 55;
            var mean = (temp3 + temp2) / 2;

            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - 85.99 MH/s, [T:" + temp1 + "C, P:252W, F:81%, E:341kH/W], 337/337 R:0%" });
            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - 85.99 MH/s, [T:" + temp2 + "C, P:252W, F:81%, E:341kH/W], 337/337 R:0%" });
            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - 85.99 MH/s, [T:" + temp3 + "C, P:252W, F:81%, E:341kH/W], 337/337 R:0%" });


            objectUnderTest.AverageTemp.Should().Be(mean, "the first hash rate should be pushed out of the queue");
            objectUnderTest.LastTemp.Should().Be(temp3, "This was the last rate we logged.");
        }

        [Fact]
        public void TempNoEntryIsZero()
        {
            var mockLogger = new Mock<ILogger<TempLogObserver>>();
            var mockListener = new Mock<IMinerLogListener>();
            var options = new TempLogObserver.Settings() { StatHistorySize = 2 };

            var objectUnderTest =
                new TempLogObserver(mockLogger.Object, Options.Create(options), mockListener.Object);

            objectUnderTest.AverageTemp.Should().Be(0, "we have no data");
        }

        [Fact]
        public void TempEventsAreBroadcast()
        {
            var mockLogger = new Mock<ILogger<TempLogObserver>>();
            var mockListener = new Mock<IMinerLogListener>();
            var options = new TempLogObserver.Settings() { StatHistorySize = 2 };

            var objectUnderTest =
                new TempLogObserver(mockLogger.Object, Options.Create(options), mockListener.Object);

            var receivedAverage = false;
            var receivedLast = false;

            const int temp1 = 100;

            objectUnderTest.MiningInformationChanged += ((sender, info) =>
            {
                if (info.Name.Equals("AverageTemp"))
                    receivedAverage = true;
                if (info.Name.Equals("LastTemp"))
                    receivedLast = true;

                info.Value.Should().Be(temp1.ToString(),
                    "the event value should be equal to the log value since there was only one log message sent.");
            });

            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - 85.99 MH/s, [T:" + temp1 + "C, P:252W, F:81%, E:341kH/W], 337/337 R:0%" });

            receivedAverage.Should().Be(true, "we should have broadcast an event.");
            receivedLast.Should().Be(true, "we should have broadcast an event.");
        }
    }
}
