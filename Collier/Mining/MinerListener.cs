using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Collier.Mining.OutputParsing;

namespace Collier.Mining
{
    public interface IMinerLogListener
    {
        event EventHandler<LogMessage> LogMessageReceived;

        void ReceiveLogMessage(object sender, LogMessage message);

    }
    public class MinerListener : IMinerLogListener
    {
        public event EventHandler<LogMessage> LogMessageReceived;

        public void ReceiveLogMessage(object sender, LogMessage message)
        {
            LogMessageReceived?.Invoke(sender, message);
        }
    }
}
