using Collier.Host;
using System;
using Collier.Monitoring.Gpu;

namespace Collier.Monitoring.Gpu
{
    public interface IGpuMonitoringBackgroundService : IBackgroundService
    {
        event EventHandler<GpuProcessEvent> ProcessEventTriggered;
    }
}