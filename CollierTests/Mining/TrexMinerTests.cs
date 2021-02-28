﻿using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.IO;
using Collier.Mining;
using Moq;
using System.Diagnostics;
using CollierService.Mining;
using Xunit;

namespace CollierTests.Mining
{
    public class TrexMinerTests
    {
        private static ILogger<DateStrippingTrexLogModifier> _fakeLogger =
            new Mock<ILogger<DateStrippingTrexLogModifier>>().Object;

        [Fact]
        public async void WebMinerIsCheckedWhenProcessIsDead()
        {
            var methodCalled = false;
            var mockWebClient = new Mock<ITrexWebClient>();
            mockWebClient.Setup(x => x.IsMiningAsync()).ReturnsAsync(() => methodCalled = true);

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<IMinerProcessFactory>();

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object, new DateStrippingTrexLogModifier(_fakeLogger));

            await miner.IsRunningAsync();

            methodCalled.Should().Be(true, "we should not invoke the web client when the process does not exist.");
        }

        [Fact]
        public async void WebMinerIsInvokedWhenProcessIsRunning()
        {
            var methodCalled = false;
            var mockWebClient = new Mock<ITrexWebClient>();
            mockWebClient.Setup(x => x.IsMiningAsync()).ReturnsAsync(() => methodCalled = true);

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<IMinerProcessFactory>();
            var process = new Mock<IProcess>();

            factory.Setup(x => x.GetNewOrExistingProcessAsync()).ReturnsAsync(process.Object);

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object, new DateStrippingTrexLogModifier(_fakeLogger));

            miner.Start();
            await miner.IsRunningAsync();

            methodCalled.Should().Be(true, "we should invoke the web client when the process is running.");
        }
        [Fact]
        public void StopKillsWhenTheProcessIsStillRunning()
        {
            var methodCalled = false;
            var mockWebClient = new Mock<ITrexWebClient>();

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<IMinerProcessFactory>();
            var process = new Mock<IProcess>();
            process.Setup(x => x.HasExited).Returns(false);
            process.Setup(x => x.Kill(It.IsAny<bool>())).Callback(() => methodCalled = true);
            var spawned = false;

            factory.Setup(x => x.GetNewOrExistingProcessAsync()).ReturnsAsync(() =>
            {
                spawned = true;
                return process.Object;
            });

            factory.Setup(x => x.CurrentProcess).Returns(() =>
            {
                if (spawned)
                    return process.Object;
                return null;
            });

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object, new DateStrippingTrexLogModifier(_fakeLogger));

            miner.Start();
            miner.Stop();

            //this is because pausing t-rex doesn't free memory.  The DAG persists and that kills gaming performance
            methodCalled.Should().Be(true, "we kill the web client when the process is running.");
        }

        [Fact]
        public void StartDoesNotCallResumeWhenTheProcessIsStillRunningButMinerNotInitialized()
        {
            var methodCallCount = 0;
            var mockWebClient = new Mock<ITrexWebClient>();
            mockWebClient.Setup(x => x.ResumeAsync()).Callback(() => methodCallCount++);
            mockWebClient.Setup(x => x.IsRunningAsync()).ReturnsAsync(false);
            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<IMinerProcessFactory>();
            var process = new Mock<IProcess>();
            process.Setup(x => x.HasExited).Returns(false);

            var spawned = false;

            factory.Setup(x => x.GetNewOrExistingProcessAsync()).ReturnsAsync(() =>
            {
                spawned = true;
                return process.Object;
            });

            factory.Setup(x => x.CurrentProcess).Returns(() =>
            {
                if (spawned)
                    return process.Object;
                return null;
            });
            process.Setup(x => x.HasExited).Returns(false);

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object, new DateStrippingTrexLogModifier(_fakeLogger));

            miner.Start();
            miner.Start();

            methodCallCount.Should().Be(0, "we should not ask the web client to resume when the web client appears unresponsive");
        }

        [Fact]
        public void StartCallsResumeOnlyWhenProcessIsResponding()
        {
            var methodCallCount = 0;
            var mockWebClient = new Mock<ITrexWebClient>();
            mockWebClient.Setup(x => x.ResumeAsync()).Callback(() => methodCallCount++);
            mockWebClient.Setup(x => x.IsRunningAsync()).ReturnsAsync(false);
            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<IMinerProcessFactory>();
            var process = new Mock<IProcess>();
            process.Setup(x => x.HasExited).Returns(false);

            var spawned = false;

            factory.Setup(x => x.GetNewOrExistingProcessAsync()).ReturnsAsync(() =>
            {
                spawned = true;
                return process.Object;
            });

            factory.Setup(x => x.CurrentProcess).Returns(() =>
            {
                if (spawned)
                    return process.Object;
                return null;
            });
            process.Setup(x => x.HasExited).Returns(false);

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object, new DateStrippingTrexLogModifier(_fakeLogger));

            miner.Start();
            miner.Start();

            methodCallCount.Should().Be(0, "we should only invoke the web client pause method when the process is running.");
        }
    }
}
