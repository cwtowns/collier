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
    public class HashRateLogObserver : IMiningInfoNotifier
    {
        private readonly ILogger<HashRateLogObserver> _logger;
        private readonly Regex _searchRegex = new Regex(@"GPU .* - (\d*\.?\d+) MH/s,");
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

        public HashRateLogObserver(ILogger<HashRateLogObserver> logger, IOptions<Settings> options, IMinerLogListener logListener)
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

                if (double.TryParse(match.Groups[1].Value, out double hashRate))
                {
                    LastHashRate = hashRate;
                    _movingStatistics.Push(hashRate);
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
            var miningInformation = new MiningInformation() { Name = "AverageHashRate", Value = AverageHashRate.ToString() };
            MiningInformationChanged?.Invoke(this, miningInformation);
            miningInformation = new MiningInformation() { Name = "LastHashRate", Value = LastHashRate.ToString() };
            MiningInformationChanged?.Invoke(this, miningInformation);
        }

        public virtual double AverageHashRate => _movingStatistics.Count == 0 ? 0 : _movingStatistics.Mean;

        public virtual double LastHashRate { get; private set; }
    }
}
