using System;
using System.Collections.Generic;
using System.Linq;
using MiningAutomater.Monitoring.Idle;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace MiningAutomater.Host
{
    public class IdleMonitoringBackgroundService : BackgroundService
    {
        private IIdleMonitorBackgroundService _realService;
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
