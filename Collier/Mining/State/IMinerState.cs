using System.Threading.Tasks;

namespace Collier.Mining.State
{
    public interface IMinerState
    {
        Task EnterStateAsync(IMiner miner);
        string StateName { get; }
    }
}
