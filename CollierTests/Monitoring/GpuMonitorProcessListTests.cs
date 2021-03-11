using FluentAssertions;
using Microsoft.Extensions.Options;
using Collier.Monitoring.Gpu;
using System.Collections.Generic;
using System.Text;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CollierTests.Monitoring
{
    public class GpuMonitorProcessListTests
    {
        [Fact]
        public void UnderLoadWhenProcessMatches()
        {
            var settings = new GpuMonitorOutputParser_ProcessList.Settings()
            {
                ValidGamePaths = new List<string>() { @"C:\Windows\System32" },
                IgnoreGamePaths = new List<string>() { @"C:\Windows\foo\bar" }
            };
            var options = Options.Create(settings);

            var objectUnderTest = new GpuMonitorOutputParser_ProcessList(new NvidiaSmiParser(), options, new Mock<ILogger<GpuMonitorOutputParser_ProcessList>>().Object);
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

            objectUnderTest.IsGpuUnderLoad(processOutput.ToString()).Should().Be(true, "there are processes in the parser that match the list.");
        }

        [Fact]
        public void IdleWhenNoPathMatch()
        {
            var settings = new GpuMonitorOutputParser_ProcessList.Settings() { ValidGamePaths = new List<string>() { @"C:\Windows\foo" } };
            var options = Options.Create(settings);
            var objectUnderTest = new GpuMonitorOutputParser_ProcessList(new NvidiaSmiParser(), options, new Mock<ILogger<GpuMonitorOutputParser_ProcessList>>().Object);
            var processOutput = new StringBuilder();

            processOutput.AppendLine(@"GPU 00000000:02:00.0");
            processOutput.AppendLine(@"    Processes");
            processOutput.AppendLine(@"        Process ID                        : 1152");
            processOutput.AppendLine(@"            Type                          : C+G");
            processOutput.AppendLine(@"            Name                          : C:\Windows\System32\dwm.exe");
            processOutput.AppendLine(@"            Used GPU Memory               : Not available in WDDM driver model");
            processOutput.AppendLine(@"        Process ID    : 3232");
            processOutput.AppendLine(@"            Type                          : X+YZ");
            processOutput.AppendLine(@"            Name                          : C:\Windows\System32\another.exe");


            objectUnderTest.IsGpuUnderLoad(processOutput.ToString()).Should().Be(false, "there are no processes in the parser that match the list.");
        }

        [Fact]
        public void IgnoreListFunctions()
        {
            var settings = new GpuMonitorOutputParser_ProcessList.Settings() { ValidGamePaths = new List<string>() { @"C:\Windows\foo" }, IgnoreGamePaths = new List<string>() { @"C:\Windows\foo\bar" } };
            var options = Options.Create(settings);
            var objectUnderTest = new GpuMonitorOutputParser_ProcessList(new NvidiaSmiParser(), options, new Mock<ILogger<GpuMonitorOutputParser_ProcessList>>().Object);
            var processOutput = new StringBuilder();

            processOutput.AppendLine(@"GPU 00000000:02:00.0");
            processOutput.AppendLine(@"    Processes");
            processOutput.AppendLine(@"        Process ID                        : 1152");
            processOutput.AppendLine(@"            Type                          : C+G");
            processOutput.AppendLine(@"            Name                          : C:\Windows\foo\bar\dwm.exe");
            processOutput.AppendLine(@"            Used GPU Memory               : Not available in WDDM driver model");
            processOutput.AppendLine(@"        Process ID    : 3232");
            processOutput.AppendLine(@"            Type                          : X+YZ");
            processOutput.AppendLine(@"            Name                          : C:\Windows\System32\another.exe");


            objectUnderTest.IsGpuUnderLoad(processOutput.ToString()).Should().Be(false, "the process should be ignored.");
        }

    }
}
