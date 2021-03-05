using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CollierService.Mining.OutputParsing
{
    /// <summary>
    /// Handles monitoring the process output for the service's internal logging diagnostics.
    /// </summary>
    public class InternalLoggingFrameworkObserver : IObserver<LogMessage>
    {
        private readonly ILogger<InternalLoggingFrameworkObserver> _logger;
        private readonly ILogger _logObserver;
        private readonly LogLevel _level;

        public InternalLoggingFrameworkObserver(ILogger<InternalLoggingFrameworkObserver> logger, ILogger logObserver, LogLevel level)
        {
            _logObserver = logObserver ?? throw new ArgumentNullException((nameof(logObserver)));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _level = level;
        }

        public virtual void OnCompleted()
        {
            _logger.LogWarning("{methodName}", "OnCompleted");
        }

        public virtual void OnError(Exception error)
        {
            _logger.LogError(error, "{methodName}", "OnError");
        }

        public virtual void OnNext(LogMessage value)
        {
            const string format = "yyyyMMdd HH:mm:ss";
            string originalMessage = value.Message;

            var firstPartOfString = originalMessage.Substring(0, Math.Min(17, originalMessage.Length));

            if (firstPartOfString.Length != format.Length)
            {
                _logger.LogDebug("{methodName} {message} {length}", "ModifyLog", "Log message is not long enough", firstPartOfString.Length);
                _logObserver.Log(_level, originalMessage);
                return;
            }

            try
            {
                DateTime result = DateTime.ParseExact(firstPartOfString, format, CultureInfo.InvariantCulture);
                _logger.LogDebug("{methodName} {message}", "ModifyLog", "modifying", firstPartOfString);
                _logObserver.Log(_level, originalMessage.Substring(Math.Min(format.Length + 1, originalMessage.Length)));
            }
            catch (Exception)
            {
                _logger.LogDebug("{methodName} {message} {firstPartOfString}", "ModifyLog", "parserError", firstPartOfString);
                _logObserver.Log(_level, originalMessage);
            }
        }

        public IDisposable Register(IObservable<LogMessage> subject)
        {
            return subject.Subscribe(this);
        }
    }
}
