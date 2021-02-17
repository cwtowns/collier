using System;

namespace MiningAutomater.Monitoring.Idle
{
    public interface IIdleMonitor
    {
        TimeSpan IdleTime { get; }
        TimeSpan Threshold { get; }

        bool IsUserIdlingPastThreshold();
    }
}