using System.Threading.Tasks;

namespace MiningAutomater.Mining
{
    public interface ITrexWebClient
    {
        Task<bool> IsMiningAsync();
        void PauseAsync();
        void ResumeAsync();
        Task<bool> IsRunningAsync();
    }
}