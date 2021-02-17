using Microsoft.Extensions.Hosting;
using MiningAutomater.Monitoring.Gpu;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiningAutomater.Host
{
    public class GpuMonitoringBackgroundService : BackgroundService
    {
        private readonly IGpuMonitoringBackgroundService _realService;
        public GpuMonitoringBackgroundService(IGpuMonitoringBackgroundService realService)
        {
            _realService = realService ?? throw new ArgumentNullException(nameof(realService));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _realService.ExecuteAsync(stoppingToken);
        }
    }
}
