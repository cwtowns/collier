using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Collier.Host;
using Collier.IO;
using Collier.Mining;
using Collier.Mining.OutputParsing;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CollierTests.Mining
{
    public class MinerEventStateTests
    {
        [Fact]
        public void DisposedMinerFiresStop()
        {
            var mockWebClient = new Mock<ITrexWebClient>();

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<IMinerProcessFactory>();
            var mockLogListener = new Mock<IMinerLogListener>();
            var mockLogObserver = new Mock<IInternalLoggingFrameworkObserver>();

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object, mockLogListener.Object, mockLogObserver.Object);
            var statePassed = false;

            miner.MiningInformationChanged += (o, e) =>
            {
                statePassed = e.Value == IMiner.MiningState.Stopped.ToString();
            };

            miner.Dispose();
            statePassed.Should().Be(true, "When we dispose of the miner we should always indicate a stopped state.");
        }

        [Fact]
        public async void StoppedMinerFiresStopWhenItsNotRunning()
        {
            var mockWebClient = new Mock<ITrexWebClient>();

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<IMinerProcessFactory>();
            var mockLogListener = new Mock<IMinerLogListener>();
            var mockLogObserver = new Mock<IInternalLoggingFrameworkObserver>();

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object, mockLogListener.Object, mockLogObserver.Object);
            var statePassed = false;

            miner.MiningInformationChanged += (o, e) =>
            {
                statePassed = e.Value == IMiner.MiningState.Stopped.ToString();
            };

            await miner.Stop();
            statePassed.Should().Be(true, "When we stop the miner we should always indicate a stopped state.");
        }

        [Fact]
        public async void StoppedMinerFiresStopWhenItsKilled()
        {
            var mockWebClient = new Mock<ITrexWebClient>();

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<IMinerProcessFactory>();

            var process = new Mock<IProcess>();
            process.Setup(x => x.HasExited).Returns(false);
            factory.Setup(x => x.CurrentProcess).Returns(process.Object);

            var mockLogListener = new Mock<IMinerLogListener>();
            var mockLogObserver = new Mock<IInternalLoggingFrameworkObserver>();

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object, mockLogListener.Object, mockLogObserver.Object);
            var statePassed = false;

            miner.MiningInformationChanged += (o, e) =>
            {
                statePassed = e.Value == IMiner.MiningState.Stopped.ToString();
            };

            await miner.Stop();
            statePassed.Should().Be(true, "When we stop the miner we should always indicate a stopped state.");
        }

        [Fact]
        public async void RunningMinerFiresStateWhenProcessIsRunning()
        {
            var mockWebClient = new Mock<ITrexWebClient>();

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<IMinerProcessFactory>();

            var process = new Mock<IProcess>();
            process.Setup(x => x.HasExited).Returns(false);
            factory.Setup(x => x.CurrentProcess).Returns(process.Object);

            var mockLogListener = new Mock<IMinerLogListener>();
            var mockLogObserver = new Mock<IInternalLoggingFrameworkObserver>();

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object, mockLogListener.Object, mockLogObserver.Object);
            var statePassed = false;

            miner.MiningInformationChanged += (o, e) =>
            {
                statePassed = e.Value == IMiner.MiningState.Running.ToString();
            };

            await miner.IsRunningAsync();
            statePassed.Should().Be(true, "When we fire the runing event when the process is running.");
        }

        [Fact]
        public async void RunningMinerFiresStateWhenProcessIsNotRunningButWebClientIs()
        {
            var mockWebClient = new Mock<ITrexWebClient>();

            mockWebClient.Setup(x => x.IsMiningAsync()).ReturnsAsync(true);

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<IMinerProcessFactory>();

            var process = new Mock<IProcess>();
            process.Setup(x => x.HasExited).Returns(true);
            factory.Setup(x => x.CurrentProcess).Returns(process.Object);

            var mockLogListener = new Mock<IMinerLogListener>();
            var mockLogObserver = new Mock<IInternalLoggingFrameworkObserver>();

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object, mockLogListener.Object, mockLogObserver.Object);
            var statePassed = false;

            miner.MiningInformationChanged += (o, e) =>
            {
                statePassed = e.Value == IMiner.MiningState.Running.ToString();
            };

            await miner.IsRunningAsync();
            statePassed.Should().Be(true, "When we fire the runing event when the web server is running.");
        }

        [Fact]
        public async void RunningMinerFiresStateWhenNothingIsRunning()
        {
            var mockWebClient = new Mock<ITrexWebClient>();

            mockWebClient.Setup(x => x.IsMiningAsync()).ReturnsAsync(true);

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<IMinerProcessFactory>();

            var process = new Mock<IProcess>();
            process.Setup(x => x.HasExited).Returns(true);
            factory.Setup(x => x.CurrentProcess).Returns(process.Object);

            var mockLogListener = new Mock<IMinerLogListener>();
            var mockLogObserver = new Mock<IInternalLoggingFrameworkObserver>();

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object, mockLogListener.Object, mockLogObserver.Object);
            var statePassed = false;

            miner.MiningInformationChanged += (o, e) =>
            {
                statePassed = e.Value == IMiner.MiningState.Running.ToString();
            };
            await miner.IsRunningAsync();
            statePassed.Should().Be(true, "When we fire the runing event when the web server is running.");
        }


        [Fact]
        public async void StartingMinerFiresStateWhenMining()
        {
            var mockWebClient = new Mock<ITrexWebClient>();

            mockWebClient.Setup(x => x.IsRunningAsync()).ReturnsAsync(true);
            mockWebClient.Setup(x => x.IsMiningAsync()).ReturnsAsync(true);

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<IMinerProcessFactory>();

            var process = new Mock<IProcess>();
            process.Setup(x => x.HasExited).Returns(false);
            factory.Setup(x => x.CurrentProcess).Returns(process.Object);

            var mockLogListener = new Mock<IMinerLogListener>();
            var mockLogObserver = new Mock<IInternalLoggingFrameworkObserver>();

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object, mockLogListener.Object, mockLogObserver.Object);
            var statePassed = false;

            miner.MiningInformationChanged += (o, e) =>
            {
                statePassed = e.Value == IMiner.MiningState.Running.ToString();
            };

            await miner.Start();
            statePassed.Should().Be(true, "When we fire the runing event when the web server is up and mining.");
        }

        [Fact]
        public async void StartingMinerFiresStateWhenPaused()
        {
            var mockWebClient = new Mock<ITrexWebClient>();

            mockWebClient.Setup(x => x.IsRunningAsync()).ReturnsAsync(true);
            mockWebClient.Setup(x => x.IsMiningAsync()).ReturnsAsync(false);

            var logger = new Mock<ILogger<TrexMiner>>();
            var settings = new TrexMiner.Settings();
            var factory = new Mock<IMinerProcessFactory>();

            var process = new Mock<IProcess>();
            process.Setup(x => x.HasExited).Returns(false);
            factory.Setup(x => x.CurrentProcess).Returns(process.Object);

            var mockLogListener = new Mock<IMinerLogListener>();
            var mockLogObserver = new Mock<IInternalLoggingFrameworkObserver>();

            var miner = new TrexMiner(logger.Object, Options.Create(settings), mockWebClient.Object, factory.Object, mockLogListener.Object, mockLogObserver.Object);
            var statePassed = false;

            miner.MiningInformationChanged += (o, e) =>
            {
                statePassed = e.Value == IMiner.MiningState.Paused.ToString();
            };

            await miner.Start();
            statePassed.Should().Be(true, "We should fire the paused event when the web server is up but not mining.");
        }

    }
}
