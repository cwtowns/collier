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

        private readonly ILogger<TrexMiner> _logger;
        private readonly Settings _settings;
        private readonly ITrexWebClient _webClient;
        private readonly IMinerProcessFactory _processFactory;

        public TrexMiner(ILogger<TrexMiner> logger, IOptions<Settings> settings, ITrexWebClient webClient, IMinerProcessFactory processFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _webClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            _processFactory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));

            _settings.ExeFileName = _settings.ExeFileName ?? string.Empty;
            _settings.ExeLocation = _settings.ExeLocation ?? string.Empty;
            _settings.ExeArguments = _settings.ExeArguments ?? string.Empty;
        }

        public void Dispose()
        {
            var process = _processFactory.CurrentProcess;

            if (process == null || process.HasExited)
                return;

            process.Kill(true);
        }

        public async Task<bool> IsRunningAsync()
        {
            var process = _processFactory.CurrentProcess;

            if (process == null)
                return await _webClient.IsMiningAsync();

            if (!process.HasExited)
                return true;

            return await _webClient.IsMiningAsync();
        }

        public async void Start()
        {
            var process = _processFactory.CurrentProcess;

            if (process == null || process.HasExited)
            {
                process = await _processFactory.GetNewOrExistingProcessAsync();
                process.OutputDataReceived += (sender, a) => _logger.LogInformation(a.Data);
                process.Start();
                process.BeginOutputReadLine();

                return;
            }

            if (!process.HasExited)
            {
                await _webClient.ResumeAsync();
            }
        }

        public void Stop()
        {
            var process = _processFactory.CurrentProcess;

            if (process == null || process.HasExited)
                return;

            process.Kill(true);
        }
    }
}
