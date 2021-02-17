using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.IO;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Collier.Mining
{

    //to pause you have to specify the exact GPU  like with this:   http://127.0.0.1:4067/control?pause=true:0
    //to check status and see if I am mining, the hashrate property will be 0 or non zero.
    public class TrexMiner : IMiner, IDisposable
    {
        public class Settings
        {
            public Settings()
            {
                ExeArguments = ExeLocation = ExeFileName = "";
            }
            public string ExeFileName { get; set; }
            public string ExeLocation { get; set; }
            public string ExeArguments { get; set; }
        }

        private readonly string _fullyQualfiedMiner;
        private IProcess _minerProcess;
        private readonly ILogger<TrexMiner> _logger;
        private readonly Settings _settings;
        private readonly ITrexWebClient _webClient;
        private readonly ProcessFactory _processFactory;

        public TrexMiner(ILogger<TrexMiner> logger, IOptions<Settings> settings, ITrexWebClient webClient, ProcessFactory processFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _webClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            _processFactory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));

            _settings.ExeFileName = _settings.ExeFileName ?? string.Empty;
            _settings.ExeLocation = _settings.ExeLocation ?? string.Empty;
            _settings.ExeArguments = _settings.ExeArguments ?? string.Empty;

            _fullyQualfiedMiner = System.IO.Path.Combine(_settings.ExeLocation, _settings.ExeFileName);
        }

        public void Dispose()
        {
            if (_minerProcess == null || _minerProcess.HasExited)
                return;

            _minerProcess.Kill(true);
        }

        public async Task<bool> IsRunningAsync()
        {
            if (_minerProcess == null)
                return false;

            if (_minerProcess.HasExited == true)
                return false;

            return await _webClient.IsRunningAsync();
        }

        public void Start()
        {
            if (_minerProcess != null && !_minerProcess.HasExited)
            {
                _webClient.ResumeAsync();
                return;
            }

            if (_minerProcess != null)
                return;
            _minerProcess = _processFactory.GetProcess(new ProcessStartInfo
            {
                FileName = _fullyQualfiedMiner,
                Arguments = _settings.ExeArguments,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });

            _minerProcess.OutputDataReceived += (sender, a) => _logger.LogInformation(a.Data);

            _minerProcess.Start();
            _minerProcess.BeginOutputReadLine();
        }

        public void Stop()
        {
            if (_minerProcess == null || _minerProcess.HasExited)
                return;

            _webClient.PauseAsync();
        }

        public void Kill()
        {
            if (_minerProcess == null || _minerProcess.HasExited)
                return;

            _minerProcess.Kill(true);
        }
    }
}
