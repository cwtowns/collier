using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using CollierService.Monitoring.Gpu;
using Microsoft.Extensions.Logging;

namespace Collier.Monitoring.Gpu
{
    //IGpuMonitorOutputParser<EventClass>
    public class GpuMonitorOutputParser_ProcessList : IGpuMonitorOutputParser, IGpuProcessMonitor<GpuProcessEvent>
    {
        public class Settings
        {
            public ICollection<string> ValidGamePaths { get; set; } = new List<string>();
        }

        private readonly INvidiaSmiParser _parser;
        private readonly Settings _settings;
        private readonly ILogger<GpuMonitorOutputParser_ProcessList> _logger;

        public event EventHandler<GpuProcessEvent> GpuActivityNoticed;

        public GpuMonitorOutputParser_ProcessList(INvidiaSmiParser parser, IOptions<Settings> settings, ILogger<GpuMonitorOutputParser_ProcessList> logger)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _settings = settings.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual void CheckGpuOutput(string output)
        {
            IsGpuUnderLoad(output);
        }
        public bool IsGpuUnderLoad(string output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var eventList = GetMonitoredProcesses(output);

            //todo should this be async
            var e = new GpuProcessEvent(eventList);
            GpuActivityNoticed?.Invoke(this, e);

            return eventList.Count > 0;
        }

        public virtual IList<string> GetMonitoredProcesses(string output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            var jObject = _parser.Parse(output);

            var eventList = new List<string>();

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

                        var val = processProperty.Value;

                        if (val["Name"] == null)
                            continue;

                        string app = val.Value<string>("Name");

                        if (app.Trim().StartsWith(s.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogDebug("found matching process {processPath}.", app);
                            eventList.Add(app);
                        }
                    }
                }
            }

            return eventList;
        }
    }
}

