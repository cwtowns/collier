using System.Threading.Tasks;

namespace Collier.Monitoring.Gpu
{
    public interface IGpuMonitor
    {
        Task<bool> IsGpuUnderLoadAsync();
    }
}