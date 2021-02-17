using Collier.Host;
using System;

namespace Collier.Monitoring.Gpu
{
    public interface IGpuMonitoringBackgroundService : IBackgroundService
    {
        event EventHandler<GpuIdleEvent> IdleThresholdReached;
    }
}