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

namespace CollierTests.Monitoring
{
    public class GpuMonitorBackgroundServiceTests
    {
        [Fact]
        public async void EventShouldHaveNoProcessesWhenNoneAreRunning()
        {
            var monitor = new Mock<IGpuMonitor>();
            var minerMock = new Mock<IMiner>();
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

            minerMock.Verify(x => x.TransitionToStateAsync(It.IsAny<MinerStartedFromNoGaming>()), Times.Once, "no processes match the list so we should start mining.");
        }

        [Fact]
        public async void NotificationSentWhenProcessesArePresent()
        {
            var monitor = new Mock<IGpuMonitor>();
            var minerMock = new Mock<IMiner>();
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

            minerMock.Verify(x => x.TransitionToStateAsync(It.IsAny<MinerStoppedFromGaming>()), Times.Once, "two processes match the list so we should stop mining.");
        }

        [Fact]
        public void ProcessEventWithProcessesShouldStopMining()
        {
            var minerMock = new Mock<IMiner>();
            var settings = new GpuMonitoringBackgroundService.Settings() { PollingIntervalInSeconds = 1 };
            var parser = new NvidiaSmiParser();

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
            minerMock.Verify(x => x.TransitionToStateAsync(It.IsAny<MinerStoppedFromGaming>()), Times.Once, "a process event with gaming content should stop mining");

            //minerMock.Verify(x => x.Stop(), Times.AtLeastOnce, "a process event with content should stop gaming.");
        }

        [Fact]
        public void ProcessEventWithNoProcessesShouldStartMining()
        {
            var minerMock = new Mock<IMiner>();
            var settings = new GpuMonitoringBackgroundService.Settings() { PollingIntervalInSeconds = 1 };
            var parser = new NvidiaSmiParser();


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

            minerMock.Verify(x => x.TransitionToStateAsync(It.IsAny<MinerStartedFromNoGaming>()), Times.Once, "a process gaming information should start mining.");
        }
    }


}
