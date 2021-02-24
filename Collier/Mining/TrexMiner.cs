using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.IO;
using System;
using System.Diagnostics;
using System.Threading;
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

            public int StartupDelayInMs { get; set; }
            public int StartupDelayAttempts { get; set; }
        }

        private readonly ILogger<TrexMiner> _logger;
        private readonly Settings _settings;
        private readonly ITrexWebClient _webClient;
        private readonly IMinerProcessFactory _processFactory;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

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
            _lock.Wait();
            try
            {
                var process = _processFactory.CurrentProcess;

                if (process == null || process.HasExited)
                    return;

                process.Kill(true);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<bool> IsRunningAsync()
        {
            await _lock.WaitAsync();
            try
            {
                var process = _processFactory.CurrentProcess;

                if (process == null)
                    return await _webClient.IsMiningAsync();

                if (!process.HasExited)
                    return true;

                return await _webClient.IsMiningAsync();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async void Start()
        {
            await _lock.WaitAsync();
            try
            {
                var process = _processFactory.CurrentProcess;

                if (process == null || process.HasExited)
                {
                    if (process == null)
                        _logger.LogInformation("Spawning new process because old process is null.");
                    else
                        _logger.LogInformation("Spawning new process because old process has exited.");
                    process = await _processFactory.GetNewOrExistingProcessAsync();
                    process.OutputDataReceived += (sender, a) =>
                    {
                        if (!string.IsNullOrEmpty(a.Data))
                            _logger.LogInformation(a.Data);
                    };
                    process.Start();
                    process.BeginOutputReadLine();
                    _logger.LogDebug("process started, waiting for success status");

                    for (int x = 0; x < _settings.StartupDelayAttempts; x++)
                    {
                        var isRunning = await _webClient.IsRunningAsync();

                        if (isRunning)
                        {
                            _logger.LogInformation("Miner has completed starting.");
                            return;
                        }

                        _logger.LogInformation("Waiting for miner process to completely start...");
                        await Task.Delay(_settings.StartupDelayInMs);
                    }

                    _logger.LogError("Miner did not start after a certain number of attempts.  Try increasing StartupDelayInMs or StartupDelayAttempts in appsettings.json.");
                    return;
                }

                _logger.LogInformation("Process already exists.");

                if (!process.HasExited)
                {
                    var isRunning = await _webClient.IsRunningAsync();

                    if (!isRunning)
                    {
                        _logger.LogWarning("Miner process is up and has not exited but isn't running yet.  " +
                            "Spawning of the process should have waited for this state to exist.  Is there an earlier error logged here?");
                        return;
                    }

                    if (await _webClient.IsMiningAsync())
                    {
                        _logger.LogInformation("Process has not exited and it might be paused, asking existing process to resume.");
                        await _webClient.ResumeAsync();
                    }
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        public void Stop()
        {
            _lock.Wait();
            try
            {
                var process = _processFactory.CurrentProcess;

                if (process == null || process.HasExited)
                {
                    _logger.LogDebug("Stop requested but process has already exited.");
                    return;
                }

                _logger.LogInformation("Stop requested and killing full process tree");
                process.Kill(true);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
