namespace MiningAutomater.Monitoring.Gpu
{
    public interface IGpuMonitorOutputParser
    {
        bool IsGpuUnderLoad(string output);
    }
}
