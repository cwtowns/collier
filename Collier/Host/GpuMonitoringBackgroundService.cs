using Microsoft.Extensions.Hosting;
using Collier.Monitoring.Gpu;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Collier.Host
{
    public class GpuMonitoringBackgroundService : BackgroundService
    {
        private readonly IBackgroundService<GpuMonitoringBackgroundService> _realService;
        public GpuMonitoringBackgroundService(IBackgroundService<GpuMonitoringBackgroundService> realService)
        {
            _realService = realService ?? throw new ArgumentNullException(nameof(realService));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _realService.ExecuteAsync(stoppingToken);
        }
    }
}
