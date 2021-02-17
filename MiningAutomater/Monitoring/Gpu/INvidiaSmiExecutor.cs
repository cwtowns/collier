using System.Threading.Tasks;

namespace MiningAutomater.Monitoring.Gpu
{
    public interface INvidiaSmiExecutor
    {
        bool HasErrored { get; }

        Task<string> ExecuteCommandAsync();
    }
}