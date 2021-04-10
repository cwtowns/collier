using Collier.Mining.State;
using System.Threading.Tasks;

namespace Collier.Mining
{
    public interface IMiner
    {
        Task Stop();
        Task Start();

        Task<bool> IsRunningAsync();

        Task<bool> TransitionToStateAsync(IMinerState state);

        IMinerState CurrentState { get; set; }
    }
}
