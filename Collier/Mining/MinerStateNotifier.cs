using Collier.Mining.OutputParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Collier.Mining
{
    public class MinerStateNotifier : IMiningInfoNotifier
    {
        public event EventHandler<MiningInformation> MiningInformationChanged;

        private readonly string _eventName = "MiningState";

        public MinerStateNotifier()
        {
            CurrentState = IMiner.MiningState.Unknown;
        }

        public IMiner.MiningState CurrentState
        {
            get; set;
        }

        public void Notify()
        {
            MiningInformationChanged?.Invoke(this, new MiningInformation() { Name = _eventName, Value = CurrentState.ToString() });
        }
    }
}
