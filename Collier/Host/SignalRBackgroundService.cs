using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Collier.Hubs;
using Collier.Mining;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Collier.Host
{
    public class SignalRBackgroundService : BackgroundService
    {
        private readonly IEnumerable<IMiningInfoNotifier> _miningInfoBroadcasters;
        private readonly IHubContext<CollierHub> _hubContext;
        private readonly ILogger<SignalRBackgroundService> _logger;

        public SignalRBackgroundService(ILogger<SignalRBackgroundService> logger, IHubContext<CollierHub> hubContext, IEnumerable<IMiningInfoNotifier> miningInfoBroadcasters)
        {
            _miningInfoBroadcasters =
                miningInfoBroadcasters ?? throw new ArgumentNullException(nameof(miningInfoBroadcasters));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("{methodName} {message}", "ExecuteAsync", "starting");

            foreach (var infoBroadcaster in _miningInfoBroadcasters)
            {
                _logger.LogInformation("{methodName} {message}", "ExecuteAsync", "Adding event listener for " + infoBroadcaster.GetType().FullName);
                infoBroadcaster.MiningInformationChanged += InfoReceived;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(int.MaxValue, stoppingToken);
            }
        }

#pragma warning disable 1998
        private async void InfoReceived(object sender, MiningInformation eventInfo)
        {
            try
            {
                _logger.LogDebug("{methodName} event:  {eventName} value:  {eventValue}", "InfoReceived", eventInfo.Name, eventInfo.Value);
#pragma warning disable 4014
                _hubContext.Clients.All.SendAsync(eventInfo.Name, eventInfo.Value);
#pragma warning restore 4014
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{methodName} {eventName} {message}", "InfoReceived", eventInfo.Name, "unexpected error");
            }
        }
#pragma warning restore 1998
    }
}
