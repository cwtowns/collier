using System;

namespace Collier.Monitoring.Gpu
{
    public class GpuIdleEvent
    {
        public GpuIdleEvent(DateTime startTime, bool idle)
        {
            IdleStartTime = startTime;
            IsIdle = idle;
        }
        public DateTime IdleStartTime { get; }
        public bool IsIdle { get; }
        public TimeSpan TimeSince => TimeSpan.FromSeconds((DateTime.Now - IdleStartTime).TotalSeconds);
    }
}
