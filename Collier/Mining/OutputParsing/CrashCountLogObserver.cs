using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Collier.Host;
using MathNet.Numerics.Statistics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Collier.Mining.OutputParsing
{
    public class CrashCountLogObserver : IMiningInfoNotifier
    {
        private readonly Regex _searchRegex = new Regex(@"WD: GPU#\d*: (\d*)");
        private readonly string _crashIdentifier = "GPU CRASH LIST";
        private readonly string _startupString = "ApiServer: Telnet server started on";
        private bool isNextMessageCrashCount = false;
        private readonly ILogger<CrashCountLogObserver> _logger;

        public event EventHandler<MiningInformation> MiningInformationChanged;

        public CrashCountLogObserver(ILogger<CrashCountLogObserver> logger, IMinerLogListener logListener)
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
                if (message.Message.StartsWith(_startupString))
                {
                    isNextMessageCrashCount = false;
                    CrashCount = 0;
                    Notify();
                    return;
                }

                if (isNextMessageCrashCount)
                {
                    //TODO also need to look for the startup log message here, then I can actually deal with counting correctly
                    var match = _searchRegex.Match(message.Message);

                    if (!match.Success)
                    {
                        _logger.LogWarning("{methodName} {message} {logMessage}", "ReceiveLogMessage", "unexpected message received when crash count was supposed to be next:  ", message.Message);
                        isNextMessageCrashCount = false;
                        return;
                    }

                    if (int.TryParse(match.Groups[1].Value, out int crashCountFromLog))
                    {
                        if (crashCountFromLog != CrashCount)
                        {
                            CrashCount = crashCountFromLog;
                            Notify();
                        }
                    }
                    else
                        _logger.LogWarning("{methodName} {message}", "ReceiveLogMessage", "Unable to parse calculated value " + match.Groups[0].Value);
                    return;
                }

                isNextMessageCrashCount = message.Message.IndexOf(_crashIdentifier) >= 0;
                return;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{methodName} {message}", "ReceiveLogMessage", "unexpected error");
            }
        }
#pragma warning restore 1998

        public virtual void Notify()
        {
            var miningInformation = new MiningInformation() { Name = "CurrentCrashCount", Value = CrashCount.ToString() };
            MiningInformationChanged?.Invoke(this, miningInformation);
        }

        public virtual int CrashCount { get; private set; }
    }
}
