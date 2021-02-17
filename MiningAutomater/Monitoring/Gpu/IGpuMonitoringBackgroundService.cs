using MiningAutomater.Host;
using System;

namespace MiningAutomater.Monitoring.Gpu
{
    public interface IGpuMonitoringBackgroundService : IBackgroundService
    {
        event EventHandler<GpuIdleEvent> IdleThresholdReached;
    }
}