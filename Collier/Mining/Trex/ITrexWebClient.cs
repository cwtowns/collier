using System.Threading.Tasks;

namespace Collier.Mining.Trex
{
    public interface ITrexWebClient
    {
        Task<bool> IsMiningAsync();
        Task PauseAsync();
        Task ResumeAsync();
        Task ShutdownAsync();

        Task<bool> IsRunningAsync();

    }
}