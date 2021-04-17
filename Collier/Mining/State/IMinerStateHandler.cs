using System.Threading.Tasks;
using Collier.Mining.State;

namespace Collier.Mining.Trex.State
{
    public interface IMinerStateHandler : IMiningInfoNotifier
    {
        Task<bool> TransitionToStateAsync(IMinerState state);
    }
}
