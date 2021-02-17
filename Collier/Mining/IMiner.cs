using System.Threading.Tasks;

namespace MiningAutomater.Mining
{
    public interface IMiner
    {
        void Stop();
        void Start();
        Task<bool> IsRunningAsync();
    }
}
