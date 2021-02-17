using Microsoft.Extensions.Hosting;
using Collier.Monitoring.Idle;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Collier.Host
{
    public class IdleMonitoringBackgroundService : BackgroundService
    {
        private readonly IIdleMonitorBackgroundService _realService;
        public IdleMonitoringBackgroundService(IIdleMonitorBackgroundService realService)
        {
            _realService = realService ?? throw new ArgumentNullException(nameof(realService));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _realService.ExecuteAsync(stoppingToken);
        }
    }
}
