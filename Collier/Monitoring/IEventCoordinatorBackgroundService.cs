using Collier.Host;
using Collier.Monitoring.Gpu;
using Collier.Monitoring.Idle;

namespace Collier.Monitoring
{
    public interface IEventCoordinatorBackgroundService : IBackgroundService
    {
        void CheckForSystemIdle();
        void GpuEventReceived(object o, GpuIdleEvent e);
        void IdleEventReceived(object o, IdleEvent e);
        bool IsSystemIdle();
    }
}