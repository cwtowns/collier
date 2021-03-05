using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Collier.Host;
using Collier.Mining;
using Microsoft.AspNetCore.SignalR;

namespace CollierService.Hubs
{
    public class CollierHub : Hub<ICollierClient>
    {
        private readonly IApplicationCancellationTokenFactory _cancellationTokenFactory;
        private readonly IMiner _miner;
        public CollierHub(IApplicationCancellationTokenFactory cancellationTokenFactory, IMiner miner)
        {
            _cancellationTokenFactory = cancellationTokenFactory ??
                                        throw new ArgumentNullException(nameof(cancellationTokenFactory));
            _miner = miner ?? throw new ArgumentNullException(nameof(miner));
        }

        public void StopMiner()
        {
            _miner.Stop();
        }

        public void StartMiner()
        {
            _miner.Start();
        }

        public void Shutdown()
        {
            _cancellationTokenFactory.GetCancellationSource().Cancel();
        }
    }
}
