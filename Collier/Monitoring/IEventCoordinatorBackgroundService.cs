using MiningAutomater.Host;
using MiningAutomater.Monitoring.Gpu;
using MiningAutomater.Monitoring.Idle;

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