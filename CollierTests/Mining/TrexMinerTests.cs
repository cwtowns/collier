﻿using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.IO;
using Collier.Mining;
using Moq;
using System.Diagnostics;
using Xunit;

namespace CollierTests.Mining
{
    public class TrexMinerTests
    {
        [Fact]
        public async void WebMinerIsCheckedWhenProcessIsDead()
        {
            var methodCalled = false;
            var mockWebClient = new Mock<ITrexWebClient>();
            mockWebClient.Setup(x => x.IsMiningAsync()).ReturnsAsync(() => methodCalled = true);

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<IMinerProcessFactory>();

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object);

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

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object);

            miner.Start();
            miner.Start();

            methodCallCount.Should().Be(1, "we should invoke the web client pause method when the process is running.");
        }
    }
}
