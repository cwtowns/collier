using Collier.Host;
using System;
using CollierService.Monitoring.Gpu;

namespace Collier.Monitoring.Gpu
{
    public interface IGpuMonitoringBackgroundService : IBackgroundService
    {
        event EventHandler<GpuIdleEvent> IdleThresholdReached;
    }

    public interface IGpuMonitoringBackgroundService2 : IBackgroundService
    {
        event EventHandler<GpuProcessEvent> ProcessEventTriggered;
    }
}