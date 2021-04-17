using Collier.Mining.State;
using Collier.Mining.Trex;
using Collier.Mining.Trex.State;
using System.Threading.Tasks;

namespace Collier.Mining
{
    public interface IMiner
    {
        Task Stop();
        Task Start();

        Task<bool> IsRunningAsync();

        IMinerState CurrentState { get; set; }

        IMinerStateHandler StateHandler { get; }
    }
}
