using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Collier.Host;
using Collier.Mining;
using Collier.Mining.OutputParsing;
using Microsoft.AspNetCore.SignalR;

namespace Collier.Hubs
{
    public class CollierHub : Hub<ICollierClient>
    {
        private readonly IApplicationCancellationTokenFactory _cancellationTokenFactory;
        private readonly IMiner _miner;
        private readonly IEnumerable<IMiningInfoNotifier> _miningInfoNotifiers;
        public CollierHub(IApplicationCancellationTokenFactory cancellationTokenFactory, IMiner miner, IEnumerable<IMiningInfoNotifier> miningInfoBroadcasters)
        {
            _cancellationTokenFactory = cancellationTokenFactory ??
                                        throw new ArgumentNullException(nameof(cancellationTokenFactory));
            _miner = miner ?? throw new ArgumentNullException(nameof(miner));
            _miningInfoNotifiers = miningInfoBroadcasters ?? throw new ArgumentNullException(nameof(miningInfoBroadcasters));
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

        public override async Task OnConnectedAsync()
        {
            await Task.Run(() =>
            {
                foreach (var i in _miningInfoNotifiers)
                {
                    i.Notify();
                }
            });
        }
    }
}
