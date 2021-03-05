using System;
using System.Threading;

namespace Collier.Host
{
    public interface IApplicationCancellationTokenFactory
    {
        CancellationToken GetCancellationToken();
        CancellationTokenSource GetCancellationSource();
    }

    public class DefaultCancellationTokenFactory : IApplicationCancellationTokenFactory
    {
        private readonly CancellationTokenSource _source;
        public DefaultCancellationTokenFactory(CancellationTokenSource tokenSource)
        {
            _source = tokenSource;
        }

        public CancellationTokenSource GetCancellationSource()
        {
            return _source;
        }

        public virtual CancellationToken GetCancellationToken()
        {
            return _source.Token;
        }
    }
}