using System;

namespace MiningAutomater.Monitoring.Idle
{
    public class IdleEvent
    {
        public IdleEvent(DateTime startTime, bool idle)
        {
            IdleStartTime = startTime;
            IsIdle = idle;
        }
        public DateTime IdleStartTime { get; }

        public TimeSpan TimeSince => TimeSpan.FromSeconds((DateTime.Now - IdleStartTime).TotalSeconds);

        public bool IsIdle { get; }
    }
}
