using System.Threading.Tasks;

namespace Collier.Mining
{
    public interface ITrexWebClient
    {
        Task<bool> IsRunningAsync();
        void PauseAsync();
        void ResumeAsync();
    }
}