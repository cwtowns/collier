using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Collier.Mining.OutputParsing
{
    public class ExternalLoggingFrameworkObserver : IMiningInfoNotifier
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

        public virtual void Notify()
        {
            //no-op for now, maybe this tracks and keeps some log information for the future (last 10 lines or so)
        }
    }
}
