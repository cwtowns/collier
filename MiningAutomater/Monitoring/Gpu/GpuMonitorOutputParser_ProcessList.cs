using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace MiningAutomater.Monitoring.Gpu
{
    public class GpuMonitorOutputParser_ProcessList : IGpuMonitorOutputParser
    {
        public class Settings
        {
            public ICollection<string> ValidGamePaths { get; set; }
        }

        private readonly INvidiaSmiParser _parser;
        private readonly Settings _settings;

        public GpuMonitorOutputParser_ProcessList(INvidiaSmiParser parser, IOptions<Settings> settings)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _settings = settings.Value;
        }

        public bool IsGpuUnderLoad(string output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var jObject = _parser.Parse(output);

            foreach (var s in _settings.ValidGamePaths)
            {
                foreach (var firstProperty in jObject.Properties())
                {
                    if (!firstProperty.Name.StartsWith("GPU"))
                        continue;

                    var firstValue = firstProperty.Value;

                    if (firstValue is not JObject)
                        continue;

                    var processes = firstValue.SelectToken("Processes") as JObject;

                    foreach (var processProperty in processes.Properties())
                    {
                        if (!processProperty.Name.StartsWith("Process ID_"))
                            continue;
                    }
                }
            }

            return false;
        }
    }
}

