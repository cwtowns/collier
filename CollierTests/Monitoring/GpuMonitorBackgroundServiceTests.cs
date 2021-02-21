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
using CollierService.Monitoring.Gpu;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CollierTests.Monitoring
{
    public class GpuMonitorBackgroundServiceTests
    {
        [Fact]
        public async void StartingTheServiceStartsTheMiner()
        {
            var monitor = new Mock<IGpuMonitor>();
            monitor.Setup(x => x.IsGpuUnderLoadAsync()).ReturnsAsync(false);
            var minerMock = new Mock<IMiner>();

            var settings = new GpuMonitoringBackgroundService.Settings() { PollingIntervalInSeconds = 1 };

            var backgroundService = new GpuMonitoringBackgroundService(
                new Mock<ILogger<GpuMonitoringBackgroundService>>().Object,
                Options.Create(settings), minerMock.Object,
                new Mock<INvidiaSmiExecutor>().Object, new Mock<IGpuProcessMonitor<GpuProcessEvent>>().Object);

            var source = new CancellationTokenSource();
            source.Cancel();
            await backgroundService.ExecuteAsync(source.Token);


            minerMock.Verify(x => x.Start(), Times.AtLeastOnce, "miner should start when the background monitoring service starts to avoid rogue background processes.");
        }

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
            var eventHandlerDelegate = new System.EventHandler<GpuProcessEvent>(action);
            backgroundService.ProcessEventTriggered += eventHandlerDelegate;

            await backgroundService.DoTaskWork();

            capturedEvent.ActiveProcesses.Count.Should().Be(0, "because no processes match the watch list.");
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
            var eventHandlerDelegate = new System.EventHandler<GpuProcessEvent>(action);
            backgroundService.ProcessEventTriggered += eventHandlerDelegate;

            await backgroundService.DoTaskWork();

            capturedEvent.ActiveProcesses.Count.Should().Be(2, "because we said two processes were running in the watch list.");
        }
    }


}
