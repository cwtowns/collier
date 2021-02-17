using Collier.Host;
using System;

namespace Collier.Monitoring.Idle
{
    public interface IIdleMonitorBackgroundService : IBackgroundService
    {
        event EventHandler<IdleEvent> IdleThresholdReached;

        void CheckIdleState();
    }
}