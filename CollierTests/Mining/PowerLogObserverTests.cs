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
    public class PowerLogObserverTests
    {
        [Fact]
        public void PowerBelowCapacityTest()
        {
            var mockLogger = new Mock<ILogger<PowerLogObserver>>();
            var mockListener = new Mock<IMinerLogListener>();
            var options = new PowerLogObserver.Settings() { StatHistorySize = 2 };

            var objectUnderTest =
                new PowerLogObserver(mockLogger.Object, Options.Create(options), mockListener.Object);

            const int power1 = 100;
            const int power2 = 202;
            var mean = (power1 + power2) / 2;

            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - 85.34 MH/s, [T:66C, P:" + power1 + "W, F:84%, E:341kH/W], 335/335 R:0%" });
            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - 32.11 MH/s, [T:66C, P:" + power2 + "W, F:84%, E:341kH/W], 335/335 R:0%" });

            objectUnderTest.AveragePower.Should().Be(mean, "this is how you calculate the mean");
        }

        [Fact]
        public void PowerAboveCapacityTest()
        {
            var mockLogger = new Mock<ILogger<PowerLogObserver>>();
            var mockListener = new Mock<IMinerLogListener>();
            var options = new PowerLogObserver.Settings() { StatHistorySize = 2 };

            var objectUnderTest =
                new PowerLogObserver(mockLogger.Object, Options.Create(options), mockListener.Object);

            const int power1 = 100;
            const int power2 = 202;
            const int power3 = 921;
            var mean = Convert.ToInt32(Math.Ceiling((power2 + power3) / 2.0));

            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - 85.34 MH/s, [T:66C, P:" + power1 + "W, F:84%, E:341kH/W], 335/335 R:0%" });
            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - 32.11 MH/s, [T:66C, P:" + power2 + "W, F:84%, E:341kH/W], 335/335 R:0%" });
            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - 32.11 MH/s, [T:66C, P:" + power3 + "W, F:84%, E:341kH/W], 335/335 R:0%" });


            objectUnderTest.AveragePower.Should().Be(mean, "the first hash rate should be pushed out of the queue");
            objectUnderTest.LastPower.Should().Be(power3, "This was the last rate we logged.");
        }

        [Fact]
        public void PowerNoEntryIsZero()
        {
            var mockLogger = new Mock<ILogger<PowerLogObserver>>();
            var mockListener = new Mock<IMinerLogListener>();
            var options = new PowerLogObserver.Settings() { StatHistorySize = 2 };

            var objectUnderTest =
                new PowerLogObserver(mockLogger.Object, Options.Create(options), mockListener.Object);

            objectUnderTest.AveragePower.Should().Be(0, "we have no data");
        }

        [Fact]
        public void PowerEventsAreBroadcast()
        {
            var mockLogger = new Mock<ILogger<PowerLogObserver>>();
            var mockListener = new Mock<IMinerLogListener>();
            var options = new PowerLogObserver.Settings() { StatHistorySize = 2 };

            var objectUnderTest =
                new PowerLogObserver(mockLogger.Object, Options.Create(options), mockListener.Object);

            var receivedAverage = false;
            var receivedLast = false;

            const int power1 = 100;

            objectUnderTest.MiningInformationChanged += ((sender, info) =>
            {
                if (info.Name.Equals("AveragePower"))
                    receivedAverage = true;
                if (info.Name.Equals("LastPower"))
                    receivedLast = true;

                info.Value.Should().Be(power1.ToString(),
                    "the event value should be equal to the log value since there was only one log message sent.");
            });

            objectUnderTest.ReceiveLogMessage(this, new LogMessage() { Message = "GPU #0: EVGA RTX 3080 - 85.34 MH/s, [T:66C, P:" + power1 + "W, F:84%, E:341kH/W], 335/335 R:0%" });

            receivedAverage.Should().Be(true, "we should have broadcast an event.");
            receivedLast.Should().Be(true, "we should have broadcast an event.");
        }
    }
}
