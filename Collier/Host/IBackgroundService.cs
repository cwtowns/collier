using System.Threading;
using System.Threading.Tasks;

namespace Collier.Host
{
    public interface IBackgroundService<T> : IBackgroundService { };

    public interface IBackgroundService
    {
        public Task ExecuteAsync(CancellationToken stoppingToken);
    }
}
