using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Collier.Host
{
    public class InitService : IHostedService
    {
        private readonly IEnumerable<IInitializationProcedure> _procedures;

        public InitService(IEnumerable<IInitializationProcedure> procedures)
        {
            _procedures = procedures ?? throw new ArgumentNullException(nameof(procedures));
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();

            foreach (var procedure in _procedures)
                tasks.Add(procedure.Init());

            return Task.WhenAll(tasks);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
