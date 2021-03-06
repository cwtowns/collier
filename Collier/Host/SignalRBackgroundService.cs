using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Collier.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace Collier.Host
{
    public class SignalRBackgroundService : BackgroundService
    {
        public SignalRBackgroundService(IHubContext<CollierHub> hubContext)
        {

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Task.Delay(500, stoppingToken);
            }

            return Task.CompletedTask;
        }
    }
}
