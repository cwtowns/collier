using System.Threading.Tasks;

namespace MiningAutomater.Monitoring.Gpu
{
    public interface IGpuMonitor
    {
        Task<bool> IsGpuUnderLoadAsync();
    }
}