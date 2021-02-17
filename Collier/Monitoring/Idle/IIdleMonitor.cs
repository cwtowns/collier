using System;

namespace Collier.Monitoring.Idle
{
    public interface IIdleMonitor
    {
        TimeSpan IdleTime { get; }
        TimeSpan Threshold { get; }

        bool IsUserIdlingPastThreshold();
    }
}