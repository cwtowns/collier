using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiningAutomater.IO;
using MiningAutomater.Mining;
using Moq;
using System.Diagnostics;
using Xunit;

namespace MiningAutomaterTests.Mining
{
    public class TrexMinerTests
    {
        [Fact]
        public async void WebMinerIsNotInvokedWhenProcessIsDead()
        {
            var methodCalled = false;
            var mockWebClient = new Mock<ITrexWebClient>();
            mockWebClient.Setup(x => x.IsMiningAsync()).ReturnsAsync(() => methodCalled = true);

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<ProcessFactory>();

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object);

            await miner.IsRunningAsync();

            methodCalled.Should().Be(false, "we should not invoke the web client when the process does not exist.");
        }

        [Fact]
        public async void WebMinerIsInvokedWhenProcessIsRunning()
        {
            var methodCalled = false;
            var mockWebClient = new Mock<ITrexWebClient>();
            mockWebClient.Setup(x => x.IsMiningAsync()).ReturnsAsync(() => methodCalled = true);

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<ProcessFactory>();
            var process = new Mock<IProcess>();

            factory.Setup(x => x.GetProcess(It.IsAny<ProcessStartInfo>())).Returns(process.Object);

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object);

            miner.Start();
            await miner.IsRunningAsync();

            methodCalled.Should().Be(true, "we should invoke the web client when the process is running.");
        }
        [Fact]
        public void StopCallsPauseWhenTheProcessIsStillRunning()
        {
            var methodCalled = false;
            var mockWebClient = new Mock<ITrexWebClient>();
            mockWebClient.Setup(x => x.PauseAsync()).Callback(() => methodCalled = true);

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<ProcessFactory>();
            var process = new Mock<IProcess>();

            factory.Setup(x => x.GetProcess(It.IsAny<ProcessStartInfo>())).Returns(process.Object);

            process.Setup(x => x.HasExited).Returns(false);

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object);

            miner.Start();
            miner.Stop();

            methodCalled.Should().Be(true, "we should invoke the web client pause method when the process is running.");
        }


        [Fact]
        public void StartCallsResumeWhenTheProcessIsStillRunning()
        {
            var methodCallCount = 0;
            var mockWebClient = new Mock<ITrexWebClient>();
            mockWebClient.Setup(x => x.ResumeAsync()).Callback(() => methodCallCount++);
            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<ProcessFactory>();
            var process = new Mock<IProcess>();

            factory.Setup(x => x.GetProcess(It.IsAny<ProcessStartInfo>())).Returns(process.Object);

            process.Setup(x => x.HasExited).Returns(false);

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object);

            miner.Start();
            miner.Start();

            methodCallCount.Should().Be(1, "we should invoke the web client pause method when the process is running.");
        }
    }
}
