using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Runtime.InteropServices;

namespace MiningAutomater.Monitoring.Idle
{
    public class IdleMonitor : IIdleMonitor
    {
        public class Settings
        {
            public int IdleThresholdInSeconds { get; set; }
        }

        private readonly Settings _settings;
        private readonly ILogger<IdleMonitor> _logger;

        [DllImport("user32.dll", SetLastError = false)]
        private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        [StructLayout(LayoutKind.Sequential)]
        private struct LASTINPUTINFO
        {
            public uint cbSize;
            public int dwTime;
        }

        public IdleMonitor(ILogger<IdleMonitor> logger, IOptions<Settings> settings)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings.Value));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual TimeSpan Threshold => TimeSpan.FromSeconds(_settings.IdleThresholdInSeconds);

        public virtual bool IsUserIdlingPastThreshold()
        {
            var last = IdleTime;
            var result = last.TotalSeconds.CompareTo(Threshold.TotalSeconds) > 0;

            _logger.LogDebug("LastInputInSeconds:  {lastInputSeconds} idle: {isIdle}", last.TotalSeconds, result);

            return result;
        }

        public virtual DateTime LastInput
        {
            get
            {
                DateTime bootTime = DateTime.UtcNow.AddMilliseconds(-Environment.TickCount);
                DateTime lastInput = bootTime.AddMilliseconds(LastInputTicks);
                return lastInput;
            }
        }

        public virtual TimeSpan IdleTime
        {
            get
            {
                return DateTime.UtcNow.Subtract(LastInput);
            }
        }

        public virtual int LastInputTicks
        {
            get
            {
                LASTINPUTINFO lii = new LASTINPUTINFO();
                lii.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));
                GetLastInputInfo(ref lii);
                return lii.dwTime;
            }
        }
    }
}
