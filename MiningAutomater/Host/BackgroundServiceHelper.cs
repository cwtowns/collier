using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiningAutomater.Host
{
    public class BackgroundServiceHelper<T> : BackgroundService
    {
        private IBackgroundService<T> _service;

        public BackgroundServiceHelper(IBackgroundService<T> service)
        {
            _service = service;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _service.ExecuteAsync(stoppingToken);
        }
    }
}