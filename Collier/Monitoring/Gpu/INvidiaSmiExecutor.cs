using System.Threading.Tasks;

namespace Collier.Monitoring.Gpu
{
    public interface INvidiaSmiExecutor
    {
        bool HasErrored { get; }

        Task<string> ExecuteCommandAsync();
    }
}