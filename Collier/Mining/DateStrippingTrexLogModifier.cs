using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CollierService.Mining
{
    public class DateStrippingTrexLogModifier : ITrexLogModifier
    {
        private ILogger<DateStrippingTrexLogModifier> _logger;
        public DateStrippingTrexLogModifier(ILogger<DateStrippingTrexLogModifier> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public string ModifyLog(string originalMessage)
        {
            const string format = "yyyyMMdd HH:mm:ss";

            var firstPartOfString = originalMessage.Substring(0, Math.Min(17, originalMessage.Length));

            if (firstPartOfString.Length != format.Length)
            {
                _logger.LogDebug("{methodName} {message} {length}", "ModifyLog", "Log message is not long enough", firstPartOfString.Length);
                return originalMessage;
            }

            try
            {
                DateTime result = DateTime.ParseExact(firstPartOfString, format, CultureInfo.InvariantCulture);
                _logger.LogDebug("{methodName} {message}", "ModifyLog", "modifying", firstPartOfString);
                return originalMessage.Substring(Math.Min(format.Length + 1, originalMessage.Length));
            }
            catch (Exception)
            {
                _logger.LogDebug("{methodName} {message} {firstPartOfString}", "ModifyLog", "parserError", firstPartOfString);
                return originalMessage;
            }
        }
    }
}
