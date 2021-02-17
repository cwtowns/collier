﻿using FluentAssertions;
using Microsoft.Extensions.Options;
using MiningAutomater.Monitoring.Gpu;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace MiningAutomaterTests.Monitoring
{
    public class GpuMonitorProcessListTests
    {
        [Fact]
        public void UnderLoadWhenPathMatches()
        {
            var settings = new GpuMonitorOutputParser_ProcessList.Settings() { ValidGamePaths = new List<string>() { @"C:\Windows\System32" } };
            var options = Options.Create(settings);

            var objectUnderTest = new GpuMonitorOutputParser_ProcessList(new NvidiaSmiParser(), options);
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


            objectUnderTest.IsGpuUnderLoad(processOutput.ToString()).Should().Be(false, "there are no processes in the parser that match the list.");
        }

        [Fact]
        public void IdleWhenNoPathMatch()
        {
            var settings = new GpuMonitorOutputParser_ProcessList.Settings() { ValidGamePaths = new List<string>() { @"C:\Windows\foo" } };
            var options = Options.Create(settings);
            var objectUnderTest = new GpuMonitorOutputParser_ProcessList(new NvidiaSmiParser(), options);
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

    }
}
