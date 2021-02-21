using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Collier.Host
{
    //TODO This pattern does not work, can it?
    public class BackgroundServiceHelper<T> : BackgroundService
    {
        private readonly IBackgroundService<T> _service;

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