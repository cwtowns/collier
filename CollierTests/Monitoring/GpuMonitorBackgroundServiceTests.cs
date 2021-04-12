using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.Monitoring.Gpu;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Collier.Mining;
using Xunit;
using Collier.Mining.State;
using Collier.Mining.Trex;
using Collier.Mining.Trex.State;

namespace CollierTests.Monitoring
{
    public class GpuMonitorBackgroundServiceTests
    {
        [Fact]
        public async void EventShouldHaveNoProcessesWhenNoneAreRunning()
        {
            var stateHandler = new Mock<IMinerStateHandler>();
            var monitor = new Mock<IGpuMonitor>();
            var minerMock = new Mock<IMiner>();
            minerMock.SetupGet(x => x.StateHandler).Returns(stateHandler.Object);
            monitor.Setup(x => x.IsGpuUnderLoadAsync()).ReturnsAsync(true);

            var settings = new GpuMonitoringBackgroundService.Settings() { PollingIntervalInSeconds = 1 };

            var processOutput = new StringBuilder();

            processOutput.AppendLine(@"GPU 00000000:02:00.0");
            processOutput.AppendLine(@"    Processes");
            processOutput.AppendLine(@"        Process ID                        : 1152");
            processOutput.AppendLine(@"            Type                          : C+G");
            processOutput.AppendLine(@"            Name                          : C:\Windows\System32\dwm.exe");
            processOutput.AppendLine(@"            Used GPU Memory               : Not available in WDDM driver model");
            processOutput.AppendLine(@"        Process ID   : 11522");
            processOutput.AppendLine(@"            Type                          : C+G");
            processOutput.AppendLine(@"            Name                          : C:\Windows\System32\another.exe");
            processOutput.AppendLine(@"            Used GPU Memory               : Not available in WDDM driver model");

            var parser = new NvidiaSmiParser();


            var outputParserSettings = new GpuMonitorOutputParser_ProcessList.Settings();
            outputParserSettings.ValidGamePaths.Add(@" C:\Windows\System32\AnotherDirectory");

            var outputProcessor = new GpuMonitorOutputParser_ProcessList(parser,
                Options.Create(outputParserSettings),
                new Mock<ILogger<GpuMonitorOutputParser_ProcessList>>().Object);

            var executor = new Mock<INvidiaSmiExecutor>();
            executor.Setup(x => x.ExecuteCommandAsync()).ReturnsAsync(processOutput.ToString());

            var backgroundService = new GpuMonitoringBackgroundService(
                new Mock<ILogger<GpuMonitoringBackgroundService>>().Object,
                Options.Create(settings), minerMock.Object,
                executor.Object, outputProcessor);

            GpuProcessEvent capturedEvent = null;
            var action = new Action<object, GpuProcessEvent>((o, e) => { capturedEvent = e; });

            await backgroundService.DoTaskWork();

            stateHandler.Verify(x => x.TransitionToStateAsync(It.IsAny<MinerStartedFromNoGaming>()), Times.Once, "no processes match the list so we should start mining.");
        }

        [Fact]
        public async void NotificationSentWhenProcessesArePresent()
        {
            var stateHandler = new Mock<IMinerStateHandler>();
            var monitor = new Mock<IGpuMonitor>();
            var minerMock = new Mock<IMiner>();
            monitor.Setup(x => x.IsGpuUnderLoadAsync()).ReturnsAsync(true);
            minerMock.SetupGet(x => x.StateHandler).Returns(stateHandler.Object);

            var settings = new GpuMonitoringBackgroundService.Settings() { PollingIntervalInSeconds = 1 };

            var processOutput = new StringBuilder();

            processOutput.AppendLine(@"GPU 00000000:02:00.0");
            processOutput.AppendLine(@"    Processes");
            processOutput.AppendLine(@"        Process ID                        : 1152");
            processOutput.AppendLine(@"            Type                          : C+G");
            processOutput.AppendLine(@"            Name                          : C:\Windows\System32\dwm.exe");
            processOutput.AppendLine(@"            Used GPU Memory               : Not available in WDDM driver model");
            processOutput.AppendLine(@"        Process ID   : 11522");
            processOutput.AppendLine(@"            Type                          : C+G");
            processOutput.AppendLine(@"            Name                          : C:\Windows\System32\another.exe");
            processOutput.AppendLine(@"            Used GPU Memory               : Not available in WDDM driver model");

            var parser = new NvidiaSmiParser();


            var outputParserSettings = new GpuMonitorOutputParser_ProcessList.Settings();
            outputParserSettings.ValidGamePaths.Add(@" C:\Windows\System32\ ");

            var outputProcessor = new GpuMonitorOutputParser_ProcessList(parser,
                Options.Create(outputParserSettings),
                new Mock<ILogger<GpuMonitorOutputParser_ProcessList>>().Object);

            var executor = new Mock<INvidiaSmiExecutor>();
            executor.Setup(x => x.ExecuteCommandAsync()).ReturnsAsync(processOutput.ToString());

            var backgroundService = new GpuMonitoringBackgroundService(
                new Mock<ILogger<GpuMonitoringBackgroundService>>().Object,
                Options.Create(settings), minerMock.Object,
                executor.Object, outputProcessor);

            GpuProcessEvent capturedEvent = null;
            var action = new Action<object, GpuProcessEvent>((o, e) => { capturedEvent = e; });

            await backgroundService.DoTaskWork();

            stateHandler.Verify(x => x.TransitionToStateAsync(It.IsAny<MinerStoppedFromGaming>()), Times.Once, "two processes match the list so we should stop mining.");
        }

        [Fact]
        public void ProcessEventWithProcessesShouldStopMining()
        {
            var stateHandler = new Mock<IMinerStateHandler>();
            var minerMock = new Mock<IMiner>();
            var settings = new GpuMonitoringBackgroundService.Settings() { PollingIntervalInSeconds = 1 };
            var parser = new NvidiaSmiParser();

            minerMock.SetupGet(x => x.StateHandler).Returns(stateHandler.Object);
            minerMock.Setup(x => x.IsRunningAsync()).ReturnsAsync(true);

            var outputParserSettings = new GpuMonitorOutputParser_ProcessList.Settings();
            outputParserSettings.ValidGamePaths.Add(@" C:\Windows\System32\ ");

            var outputProcessor = new GpuMonitorOutputParser_ProcessList(parser,
                Options.Create(outputParserSettings),
                new Mock<ILogger<GpuMonitorOutputParser_ProcessList>>().Object);


            var executor = new Mock<INvidiaSmiExecutor>();
            var backgroundService = new GpuMonitoringBackgroundService(
                new Mock<ILogger<GpuMonitoringBackgroundService>>().Object,
                Options.Create(settings), minerMock.Object,
                executor.Object, outputProcessor);

            backgroundService.CheckActivity(this, new GpuProcessEvent(new List<string>() { "some gaming process" }));
            stateHandler.Verify(x => x.TransitionToStateAsync(It.IsAny<MinerStoppedFromGaming>()), Times.Once, "a process event with gaming content should stop mining");
        }

        [Fact]
        public void ProcessEventWithNoProcessesShouldStartMining()
        {
            var stateHandler = new Mock<IMinerStateHandler>();
            var minerMock = new Mock<IMiner>();
            var settings = new GpuMonitoringBackgroundService.Settings() { PollingIntervalInSeconds = 1 };
            var parser = new NvidiaSmiParser();
            minerMock.SetupGet(x => x.StateHandler).Returns(stateHandler.Object);


            var outputParserSettings = new GpuMonitorOutputParser_ProcessList.Settings();
            outputParserSettings.ValidGamePaths.Add(@" C:\Windows\System32\ ");

            var outputProcessor = new GpuMonitorOutputParser_ProcessList(parser,
                Options.Create(outputParserSettings),
                new Mock<ILogger<GpuMonitorOutputParser_ProcessList>>().Object);


            var executor = new Mock<INvidiaSmiExecutor>();
            var backgroundService = new GpuMonitoringBackgroundService(
                new Mock<ILogger<GpuMonitoringBackgroundService>>().Object,
                Options.Create(settings), minerMock.Object,
                executor.Object, outputProcessor);

            backgroundService.CheckActivity(this, new GpuProcessEvent(new List<string>()));

            stateHandler.Verify(x => x.TransitionToStateAsync(It.IsAny<MinerStartedFromNoGaming>()), Times.Once, "a process gaming information should start mining.");
        }
    }


}
