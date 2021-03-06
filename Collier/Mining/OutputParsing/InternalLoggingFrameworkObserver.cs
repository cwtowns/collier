using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Collier.Mining;
using Collier.Host;
using Microsoft.Extensions.Logging;

namespace Collier.Mining.OutputParsing
{
    /// <summary>
    /// Handles monitoring the process output for the service's internal logging diagnostics.
    /// </summary>
    public class InternalLoggingFrameworkObserver : IInitializationProcedure
    {
        private readonly ILogger<InternalLoggingFrameworkObserver> _logger;
        private readonly ILogger _logObserver;

        public InternalLoggingFrameworkObserver(ILogger<InternalLoggingFrameworkObserver> logger, ILogger<IMiner> logObserver, IMinerLogListener logListener)
        {
            _logObserver = logObserver ?? throw new ArgumentNullException((nameof(logObserver)));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            logListener = logListener ?? throw new ArgumentNullException(nameof(logListener));

            logListener.LogMessageReceived += ReceiveLogMessage;
        }

        public async Task Init()
        {
            //no-op, we wired everything up in the constructor
            await Task.CompletedTask;
        }

#pragma warning disable 1998
        public virtual async void ReceiveLogMessage(object sender, LogMessage message)
        {
            try
            {
                const string format = "yyyyMMdd HH:mm:ss";
                string originalMessage = message.Message;

                var firstPartOfString = originalMessage.Substring(0, Math.Min(17, originalMessage.Length));

                if (firstPartOfString.Length != format.Length)
                {
                    _logger.LogDebug("{methodName} {message} {length}", "ModifyLog", "Log message is not long enough",
                        firstPartOfString.Length);
                    _logObserver.LogInformation(originalMessage);
                    return;
                }

                try
                {
                    DateTime result = DateTime.ParseExact(firstPartOfString, format, CultureInfo.InvariantCulture);
                    _logger.LogDebug("{methodName} {message} {strippedMessage}", "ModifyLog", "modifying", firstPartOfString);
                    _logObserver.LogInformation(originalMessage.Substring(Math.Min(format.Length + 1, originalMessage.Length)));
                }
                catch (Exception)
                {
                    _logger.LogDebug("{methodName} {message} {firstPartOfString}", "ModifyLog", "parserError",
                        firstPartOfString);
                    _logObserver.LogInformation(originalMessage);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{methodName} {message} ", "ModifyLog", "unexpected error received");
            }

        }
    }
#pragma warning restore 1998
}
