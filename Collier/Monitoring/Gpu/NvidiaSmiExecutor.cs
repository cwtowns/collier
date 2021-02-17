using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.IO;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Collier.Monitoring.Gpu
{
    public class NvidiaSmiExecutor : INvidiaSmiExecutor
    {
        public class Settings
        {
            public string SmiCommandLocation { get; set; }
        }

        private readonly Settings _settings;
        private readonly ILogger<NvidiaSmiExecutor> _logger;
        private readonly ProcessFactory _processFactory;


        public NvidiaSmiExecutor(IOptions<Settings> settings, ILogger<NvidiaSmiExecutor> logger, ProcessFactory processFactory)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _processFactory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
        }

        public virtual async Task<string> ExecuteCommandAsync()
        {
            if (!System.IO.File.Exists(_settings.SmiCommandLocation))
            {
                _logger.LogError("No file at command location {commandLocation}.", _settings.SmiCommandLocation);
                return "";
            }

            var outputBuilder = new StringBuilder();
            var commandProcess = _processFactory.GetProcess(new ProcessStartInfo
            {
                FileName = _settings.SmiCommandLocation,
                Arguments = "-q",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            commandProcess.OutputDataReceived += (sender, a) => outputBuilder.AppendLine(a.Data);

            commandProcess.Start();
            commandProcess.BeginOutputReadLine();

            await commandProcess.WaitForExitAsync();

            HasErrored = commandProcess.ExitCode > 0;

            return outputBuilder.ToString();
        }

        public virtual bool HasErrored
        {
            get; private set;
        }

    }
}
