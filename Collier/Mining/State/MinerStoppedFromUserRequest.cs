using System.Threading.Tasks;

namespace Collier.Mining.State
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class MinerStoppedFromUserRequest : IMinerState
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public async Task EnterStateAsync(IMiner miner)
        {
            if (miner.CurrentState is MinerStartedFromNoGaming || miner.CurrentState is MinerStartedFromUserRequest)
            {
                await miner.Stop();
                miner.CurrentState = this;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is MinerStoppedFromUserRequest;
        }

        public virtual string StateName => "UserPaused";
    }
}
