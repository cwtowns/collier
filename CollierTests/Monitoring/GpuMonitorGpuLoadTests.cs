using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collier.Monitoring.Gpu;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace CollierTests.Monitoring
{
    public class GpuMonitorGpuLoadTests
    {
        [Fact]
        public void GpuMonitorShowsLoad()
        {
            var settings = new GpuMonitorOutputParser_GpuLoad.Settings();

            settings.LoadThresholdForActivity = 50;

            var objectUnderTest = new GpuMonitorOutputParser_GpuLoad(new NvidiaSmiParser(), Options.Create(settings));
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("GPU 00000000:02:00.0");
            stringBuilder.AppendLine("    Product Name                          : GeForce RTX 3080");
            stringBuilder.AppendLine("    Utilization");
            stringBuilder.AppendLine("        Gpu                               : 51 %");
            stringBuilder.AppendLine("        Memory                            : 0 %");
            stringBuilder.AppendLine("        Encoder                           : 0 %");
            stringBuilder.AppendLine("        Decoder                           : 0 %");

            objectUnderTest.IsGpuUnderLoad(stringBuilder.ToString()).Should()
                .Be(true, "utilization is above the threshold");
        }

        [Fact]
        public void GpuMonitorShowsNoLoad()
        {
            var settings = new GpuMonitorOutputParser_GpuLoad.Settings();

            settings.LoadThresholdForActivity = 70;

            var objectUnderTest = new GpuMonitorOutputParser_GpuLoad(new NvidiaSmiParser(), Options.Create(settings));
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("GPU 00000000:02:00.0");
            stringBuilder.AppendLine("    Product Name                          : GeForce RTX 3080");
            stringBuilder.AppendLine("    Utilization");
            stringBuilder.AppendLine("        Gpu                               : 51 %");
            stringBuilder.AppendLine("        Memory                            : 0 %");
            stringBuilder.AppendLine("        Encoder                           : 0 %");
            stringBuilder.AppendLine("        Decoder                           : 0 %");

            objectUnderTest.IsGpuUnderLoad(stringBuilder.ToString()).Should()
                .Be(false, "utilization is below the threshold");
        }
    }


}
