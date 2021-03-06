using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Collier.Mining.OutputParsing
{
    public class ExternalLoggingFrameworkObserver : IMiningInfoBroadcaster
    {
        public event EventHandler<MiningInformation> MiningInformationChanged;

        public ExternalLoggingFrameworkObserver(IMinerLogListener logListener)
        {
            if (logListener == null)
                throw new ArgumentNullException(nameof(logListener));

            logListener.LogMessageReceived += ReceiveLogMessage;
        }

#pragma warning disable 1998
        public virtual async void ReceiveLogMessage(object sender, LogMessage message)
        {
            MiningInformationChanged?.Invoke(this, new MiningInformation() { Name = "Log", Value = message.Message });
        }
#pragma warning restore 1998
    }
}
