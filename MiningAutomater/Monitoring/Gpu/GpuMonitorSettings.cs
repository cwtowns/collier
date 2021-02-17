namespace MiningAutomater.Monitoring.Gpu
{
    public class GpuMonitoringSettings
    {
        public GpuMonitorOutputParserSettings OutputParsers { get; set; }
    }

    public class GpuMonitorOutputParserSettings
    {
        public GpuMonitorOutputParser_GpuLoad.Settings GpuLoadOutputParser { get; set; }
        public GpuMonitorOutputParser_ProcessList.Settings processListOutputParser { get; set; }

        public int PollingIntervalInSeconds { get; set; }
        public string SmiCommandLocation { get; set; }
    }
}
