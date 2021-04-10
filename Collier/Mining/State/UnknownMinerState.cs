using System.Threading.Tasks;

namespace Collier.Mining.State
{
#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    public class UnknownMinerState : IMinerState
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
    {
        public async Task EnterStateAsync(IMiner miner)
        {
            if (miner.CurrentState == null)
                miner.CurrentState = this;
        }

        public override bool Equals(object obj)
        {
            return obj is UnknownMinerState;
        }

        public virtual string StateName => "Unknown";
    }
}
