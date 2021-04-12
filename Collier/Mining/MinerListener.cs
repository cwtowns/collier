using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Collier.Mining
{

    /// <summary>
    /// An object to subscribe to in order to receive miner output log messages.
    /// All statistics about mining are obtained from the log file, so any observers
    /// will receive an instance of IMinerLogListener so they can be notified of log output
    /// </summary>
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
