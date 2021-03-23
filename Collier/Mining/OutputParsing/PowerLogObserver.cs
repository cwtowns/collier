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
    public class PowerLogObserver : IMiningInfoNotifier
    {
        private readonly ILogger<PowerLogObserver> _logger;
        private readonly Regex _searchRegex = new Regex(@"GPU .* P:(\d*)W,");
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

        public PowerLogObserver(ILogger<PowerLogObserver> logger, IOptions<Settings> options, IMinerLogListener logListener)
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

                if (int.TryParse(match.Groups[1].Value, out int power))
                {
                    LastPower = power;
                    _movingStatistics.Push(power);
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

        public void Notify()
        {
            var miningInformation = new MiningInformation() { Name = "AveragePower", Value = AveragePower.ToString() };
            MiningInformationChanged?.Invoke(this, miningInformation);
            miningInformation = new MiningInformation() { Name = "LastPower", Value = LastPower.ToString() };
            MiningInformationChanged?.Invoke(this, miningInformation);
        }

        public virtual int AveragePower => _movingStatistics.Count == 0 ? 0 : Convert.ToInt32(_movingStatistics.Mean);

        public virtual int LastPower { get; private set; }
    }
}
