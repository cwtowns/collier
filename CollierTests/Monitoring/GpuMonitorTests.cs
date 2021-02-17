using FluentAssertions;
using Microsoft.Extensions.Logging;
using Collier.Monitoring.Gpu;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace CollierTests.Monitoring
{
    public class GpuMonitorTests
    {
        [Fact]
        public async void NoMonitorShowsLoadMeansNoLoad()
        {
            var mockLogger = new Mock<ILogger<GpuMonitor>>();
            var mockExecutor = new Mock<INvidiaSmiExecutor>();
            var outputParser = new Mock<IGpuMonitorOutputParser>();

            outputParser.Setup(x => x.IsGpuUnderLoad(It.IsAny<string>())).Returns(false);

            var objectUnderTest = new GpuMonitor(mockLogger.Object, new List<IGpuMonitorOutputParser>() { outputParser.Object }, mockExecutor.Object);
            var result = await objectUnderTest.IsGpuUnderLoadAsync();
            result.Should().Be(false, "no parsers are set to return load");
        }

        [Fact]
        public async void MonitorShowsLoadMeansLoad()
        {
            var mockLogger = new Mock<ILogger<GpuMonitor>>();
            var mockExecutor = new Mock<INvidiaSmiExecutor>();
            var outputParser = new Mock<IGpuMonitorOutputParser>();

            outputParser.Setup(x => x.IsGpuUnderLoad(It.IsAny<string>())).Returns(true);

            var objectUnderTest = new GpuMonitor(mockLogger.Object, new List<IGpuMonitorOutputParser>() { outputParser.Object }, mockExecutor.Object);
            var result = await objectUnderTest.IsGpuUnderLoadAsync();
            result.Should().Be(true, "one parser is set to show load");
        }
    }
}
