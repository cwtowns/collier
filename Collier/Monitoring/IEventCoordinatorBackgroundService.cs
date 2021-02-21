using Collier.Host;
using Collier.Monitoring.Gpu;
using Collier.Monitoring.Idle;
using CollierService.Monitoring.Gpu;

namespace Collier.Monitoring
{
    public interface IEventCoordinatorBackgroundService : IBackgroundService
    {
        void CheckForSystemIdle();

        void GpuProcessEventReceived(object o, GpuProcessEvent e);
        void GpuEventReceived(object o, GpuIdleEvent e);
        void IdleEventReceived(object o, IdleEvent e);
        bool IsSystemIdle();
    }
}