using System.Threading.Tasks;

namespace MiningAutomater.Mining
{
    public interface ITrexWebClient
    {
        Task<bool> IsRunningAsync();
        void PauseAsync();
        void ResumeAsync();
    }
}