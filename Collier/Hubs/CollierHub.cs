using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Collier.Host;
using Collier.Mining;
using Collier.Mining.State;
using Microsoft.AspNetCore.SignalR;

namespace Collier.Hubs
{
    /// <summary>
    /// Server side web socket hub.  All public methods that are not overrides are publically callable
    /// by connected clients.  
    /// </summary>
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
            _miner.StateHandler.TransitionToStateAsync(new MinerStoppedFromUserRequest());
        }

        public void StartMiner()
        {
            _miner.StateHandler.TransitionToStateAsync(new MinerStartedFromUserRequest());
        }

        public void SendMinerState()
        {
            _miner.StateHandler.Notify();
        }

        /// <summary>
        /// Pushes all state information to the client when the client connects / reconnects
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.Connected();

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
