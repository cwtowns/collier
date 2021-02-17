using System.Threading;
using System.Threading.Tasks;

namespace MiningAutomater.Host
{
    public interface IBackgroundService<T> : IBackgroundService { };

    public interface IBackgroundService
    {
        public Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
