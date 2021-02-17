using Microsoft.Extensions.Hosting;
using MiningAutomater.Monitoring;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiningAutomater.Host
{
    public class EventCoordinatorBackgroundService : BackgroundService
    {
        private readonly IEventCoordinatorBackgroundService _realService;
        public EventCoordinatorBackgroundService(IEventCoordinatorBackgroundService realService)
        {
            _realService = realService ?? throw new ArgumentNullException(nameof(realService));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _realService.ExecuteAsync(stoppingToken);
        }
    }
}
