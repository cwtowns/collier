using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Collier.Mining.Trex.OutputParsing
{
    public class MinerUpdateAvailableObserver : IMiningInfoNotifier
    {
        private readonly ILogger<MinerUpdateAvailableObserver> _logger;
        private readonly Regex _searchRegex = new Regex(@".*New version of T-Rex is available");

        public event EventHandler<MiningInformation> MiningInformationChanged;

        public bool HasUpdate { get; private set; }

        public MinerUpdateAvailableObserver(ILogger<MinerUpdateAvailableObserver> logger, IMinerLogListener logListener)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            logListener = logListener ?? throw new ArgumentNullException(nameof(logListener));
            logListener.LogMessageReceived += ReceiveLogMessage;
        }

#pragma warning disable 1998
        public virtual async void ReceiveLogMessage(object sender, LogMessage message)
        {
            try
            {
                if (message.Message.StartsWith(TrexMiner.STARTUP_LOG_MESSAGE))
                {
                    HasUpdate = false;
                    Notify();
                    return;
                }

                var match = _searchRegex.Match(message.Message);
                if (!match.Success)
                    return;

                HasUpdate = true;
                Notify();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{methodName} {message}", "ReceiveLogMessage", "unexpected error");
            }
        }
#pragma warning restore 1998

        public void Notify()
        {
            var miningInformation = new MiningInformation() { Name = "MinerUpdateAvailable", Value = HasUpdate.ToString() };
            MiningInformationChanged?.Invoke(this, miningInformation);
        }
    }
}
