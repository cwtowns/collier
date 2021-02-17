using Microsoft.Extensions.Hosting;
using MiningAutomater.Host;
using MiningAutomater.Monitoring.Gpu;
using MiningAutomater.Monitoring.Idle;
using System;

namespace MiningAutomater.Monitoring
{
    public interface IEventCoordinatorBackgroundService : IBackgroundService
    {
        void CheckForSystemIdle();
        void GpuEventReceived(object o, GpuIdleEvent e);
        void IdleEventReceived(object o, IdleEvent e);
        bool IsSystemIdle();
    }
}