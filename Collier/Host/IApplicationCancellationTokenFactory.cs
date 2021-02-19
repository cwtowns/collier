using System;
using System.Threading;

namespace Collier.Host
{
    public interface IApplicationCancellationTokenFactory
    {
        CancellationToken GetCancellationToken();
    }

    public class DefaultCancellationTokenFactory : IApplicationCancellationTokenFactory
    {
        private CancellationToken _token;
        public DefaultCancellationTokenFactory(CancellationToken token)
        {
            _token = token;
        }

        public virtual CancellationToken GetCancellationToken()
        {
            return _token;
        }
    }
}