using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Collier.Host;
using Collier.IO;
using Collier.Mining;
using Collier.Mining.Trex;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CollierTests.Mining
{
    public class MinerProcessFactoryTests
    {
        [Fact]
        public async void KillCallsKillOnRemainingProcesses()
        {
            var loggerMock = new Mock<ILogger<MinerProcessFactory>>();
            var tokenFactoryMock = new Mock<IApplicationCancellationTokenFactory>();
            var processFactoryMock = new Mock<ProcessFactory>();
            var trexWebClietnMock = new Mock<ITrexWebClient>();
            var minerSettings = Options.Create(new TrexMiner.Settings());
            var webSettings = Options.Create(new TrexWebClient.Settings());

            trexWebClietnMock.Setup(x => x.IsRunningAsync()).ReturnsAsync(true);

            var runningProcess = new Mock<IProcess>();
            var processList = new List<IProcess>() { runningProcess.Object };

            processFactoryMock.Setup(x => x.GetExistingProcessList(It.IsAny<string>())).Returns(processList);

            var factoryMock = new Mock<MinerProcessFactory>(new object[]
            {
                loggerMock.Object,
                tokenFactoryMock.Object,
                processFactoryMock.Object,
                trexWebClietnMock.Object,
                minerSettings,
                webSettings
            });
            var mockProcess = new Mock<IProcess>();
            mockProcess.Setup(x => x.HasExited).Returns(true);

            factoryMock.CallBase = true;


            factoryMock.Setup(x => x.CurrentProcess).Returns(mockProcess.Object);

            await factoryMock.Object.KillAllRogueProcessesAsync();

            runningProcess.Verify(x => x.Kill(It.IsAny<bool>()), Times.AtLeastOnce,
                "Remaining processes should be manually killed.");
        }

        [Fact]
        public async void AttemptRogueWebShutdownDuringKill()
        {
            var loggerMock = new Mock<ILogger<MinerProcessFactory>>();
            var tokenFactoryMock = new Mock<IApplicationCancellationTokenFactory>();
            var processFactoryMock = new Mock<ProcessFactory>();
            var trexWebClietnMock = new Mock<ITrexWebClient>();
            var minerSettings = Options.Create(new TrexMiner.Settings());
            var webSettings = Options.Create(new TrexWebClient.Settings());

            trexWebClietnMock.Setup(x => x.IsRunningAsync()).ReturnsAsync(true);

            processFactoryMock.Setup(x => x.GetExistingProcessList(It.IsAny<string>())).Returns(new List<IProcess>());

            var factoryMock = new Mock<MinerProcessFactory>(new object[]
            {
                loggerMock.Object,
                tokenFactoryMock.Object,
                processFactoryMock.Object,
                trexWebClietnMock.Object,
                minerSettings,
                webSettings
            });
            var mockProcess = new Mock<IProcess>();
            mockProcess.Setup(x => x.HasExited).Returns(true);

            factoryMock.CallBase = true;


            factoryMock.Setup(x => x.CurrentProcess).Returns(mockProcess.Object);

            await factoryMock.Object.KillAllRogueProcessesAsync();

            trexWebClietnMock.Verify(x => x.ShutdownAsync(), Times.AtLeastOnce,
                "Web shutdown should be attempted during kill.");
        }

        [Fact]
        public async void WhenProcessIsNotRunningOtherProcessesAreKilled()
        {

            var loggerMock = new Mock<ILogger<MinerProcessFactory>>();
            var tokenFactoryMock = new Mock<IApplicationCancellationTokenFactory>();
            var processFactoryMock = new Mock<ProcessFactory>();
            var trexWebClietnMock = new Mock<ITrexWebClient>();
            var minerSettings = Options.Create(new TrexMiner.Settings());
            var webSettings = Options.Create(new TrexWebClient.Settings());

            processFactoryMock.Setup(x => x.GetExistingProcessList(It.IsAny<string>())).Returns(new List<IProcess>());

            var factoryMock = new Mock<MinerProcessFactory>(new object[]
            {
                loggerMock.Object,
                tokenFactoryMock.Object,
                processFactoryMock.Object,
                trexWebClietnMock.Object,
                minerSettings,
                webSettings
            });
            var mockProcess = new Mock<IProcess>();
            mockProcess.Setup(x => x.HasExited).Returns(true);

            factoryMock.CallBase = true;


            factoryMock.Setup(x => x.CurrentProcess).Returns(mockProcess.Object);

            await factoryMock.Object.GetNewOrExistingProcessAsync();



#pragma warning disable 4014
            factoryMock.Verify(x => x.KillRemainingMinersAsync(), Times.AtLeastOnce, "When the process is not running we should attempt to kill any rogue ones too.");
#pragma warning restore 4014
        }


        [Fact]
        public async void CurrentProcessIsReturnedWhenStillRunning()
        {

            var loggerMock = new Mock<ILogger<MinerProcessFactory>>();
            var tokenFactoryMock = new Mock<IApplicationCancellationTokenFactory>();
            var processFactoryMock = new Mock<ProcessFactory>();
            var trexWebClietnMock = new Mock<ITrexWebClient>();
            var minerSettings = Options.Create(new TrexMiner.Settings());
            var webSettings = Options.Create(new TrexWebClient.Settings());

            processFactoryMock.Setup(x => x.GetExistingProcessList(It.IsAny<string>())).Returns(new List<IProcess>());


            var factoryMock = new Mock<MinerProcessFactory>(new object[]
            {
                loggerMock.Object,
                tokenFactoryMock.Object,
                processFactoryMock.Object,
                trexWebClietnMock.Object,
                minerSettings,
                webSettings
            });
            var mockProcess = new Mock<IProcess>();
            mockProcess.Setup(x => x.HasExited).Returns(false);

            factoryMock.CallBase = true;


            factoryMock.Setup(x => x.CurrentProcess).Returns(mockProcess.Object);

            (await factoryMock.Object.GetNewOrExistingProcessAsync()).Should()
                    .Be(mockProcess.Object, "the existing process should be returned.");
        }


    }
}
