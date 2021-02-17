using MiningAutomater.Host;
using System;

namespace MiningAutomater.Monitoring.Idle
{
    public interface IIdleMonitorBackgroundService : IBackgroundService
    {
        event EventHandler<IdleEvent> IdleThresholdReached;

        void CheckIdleState();
    }
}