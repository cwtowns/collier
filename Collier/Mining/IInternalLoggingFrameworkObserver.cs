using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Collier.Mining
{
    public interface IInternalLoggingFrameworkObserver
    {
        void ReceiveLogMessage(object sender, LogMessage message);
    }

}
