using Collier.Host;
using MathNet.Numerics.Statistics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Collier.Mining.OutputParsing
{
    public class TempLogObserver : IMiningInfoNotifier
    {
        private readonly ILogger<TempLogObserver> _logger;
        private readonly Regex _searchRegex = new Regex(@"GPU .* \[T:(\d*)C,");
        private readonly MovingStatistics _movingStatistics;

        public class Settings
        {
            public Settings()
            {
                StatHistorySize = 100;
            }
            public int StatHistorySize { get; set; }
        }

        private readonly Settings _settings;

        public event EventHandler<MiningInformation> MiningInformationChanged;

        public TempLogObserver(ILogger<TempLogObserver> logger, IOptions<Settings> options, IMinerLogListener logListener)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            logListener = logListener ?? throw new ArgumentNullException(nameof(logListener));
            logListener.LogMessageReceived += ReceiveLogMessage;
            _settings = options.Value;
            _movingStatistics = new MovingStatistics(_settings.StatHistorySize);
        }

#pragma warning disable 1998
        public virtual async void ReceiveLogMessage(object sender, LogMessage message)
        {
            try
            {
                var match = _searchRegex.Match(message.Message);
                if (!match.Success)
                    return;

                if (int.TryParse(match.Groups[1].Value, out int temp))
                {
                    LastTemp = temp;
                    _movingStatistics.Push(temp);
                    Notify();
                }
                else
                    _logger.LogWarning("{methodName} {message}", "ReceiveLogMessage", "Unable to parse calculated value " + match.Groups[0].Value);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{methodName} {message}", "ReceiveLogMessage", "unexpected error");
            }
        }
#pragma warning restore 1998

        public virtual void Notify()
        {
            var miningInformation = new MiningInformation() { Name = "AverageTemp", Value = AverageTemp.ToString() };
            MiningInformationChanged?.Invoke(this, miningInformation);
            miningInformation = new MiningInformation() { Name = "LastTemp", Value = LastTemp.ToString() };
            MiningInformationChanged?.Invoke(this, miningInformation);
        }

        public virtual int AverageTemp => _movingStatistics.Count == 0 ? 0 : Convert.ToInt32(_movingStatistics.Mean);

        public virtual int LastTemp { get; private set; }
    }
}
