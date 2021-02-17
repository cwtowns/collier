using Microsoft.Extensions.Options;
using System;

namespace MiningAutomater.Monitoring.Gpu
{
    public class GpuMonitorOutputParser_GpuLoad : IGpuMonitorOutputParser
    {
        public class Settings
        {
            public long LoadThresholdForActivity { get; set; }
        }

        private readonly INvidiaSmiParser _parser;
        private readonly Settings _settings;
        public GpuMonitorOutputParser_GpuLoad(INvidiaSmiParser parser, IOptions<Settings> settings)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _settings = settings.Value;

            if (_settings.LoadThresholdForActivity <= 0)
                throw new ArgumentOutOfRangeException(nameof(_settings.LoadThresholdForActivity));
        }

        public bool IsGpuUnderLoad(string output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var jObject = _parser.Parse(output);

            foreach (var firstProperty in jObject.Properties())
            {
                if (!firstProperty.Name.StartsWith("GPU 00000000"))
                    continue;

                var gpuValue = firstProperty.Value["Utilization"].Value<string>("Gpu");

                gpuValue = gpuValue.Trim('%').Trim(' ');

                int currentLoad = 0;
                if (!int.TryParse(gpuValue, out currentLoad))
                    throw new ArgumentOutOfRangeException(nameof(gpuValue), "Gpu value is not a parsable number (" + gpuValue + ")");

                return currentLoad > _settings.LoadThresholdForActivity;
            }

            return false;
        }
    }
}
