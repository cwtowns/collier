using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Collier.IO;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Collier.Mining;
using Collier.Mining.State;
using Collier.Mining.Trex.State;

namespace Collier.Mining.Trex
{

    //to pause you have to specify the exact GPU  like with this:   http://127.0.0.1:4067/control?pause=true:0
    //to check status and see if I am mining, the hashrate property will be 0 or non zero.
    public class TrexMiner : IMiner, IDisposable
    {
        public const string STARTUP_LOG_MESSAGE = "ApiServer: Telnet server started on";

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

        private readonly IMinerLogListener _listener;

        public virtual IMinerStateHandler StateHandler { get; private set; }

        public IMinerState CurrentState { get; set; }

        public TrexMiner(ILogger<TrexMiner> logger, IOptions<Settings> settings, ITrexWebClient webClient, IMinerProcessFactory processFactory, IMinerLogListener listener, IInternalLoggingFrameworkObserver loggingFrameworkObserver)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _webClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            _processFactory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));

            _settings.ExeFileName = _settings.ExeFileName ?? string.Empty;
            _settings.ExeLocation = _settings.ExeLocation ?? string.Empty;
            _settings.ExeArguments = _settings.ExeArguments ?? string.Empty;

            _listener = listener ?? throw new ArgumentNullException(nameof(listener));

            loggingFrameworkObserver = loggingFrameworkObserver ?? throw new ArgumentNullException(nameof(loggingFrameworkObserver));

            //i dont know a better spot to do this wire up which is frustrating.  
            //it needs to happen after all dependencies are registered and before background services start
            //I think the only way I can ensure that is to have it execute as part of an object I know
            //is resolved when the miner is resolved
            _listener.LogMessageReceived += loggingFrameworkObserver.ReceiveLogMessage;

            StateHandler = new MinerStateHandler(this);
            StateHandler.TransitionToStateAsync(new UnknownMinerState());
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
            GC.SuppressFinalize(this);
        }

        public async Task<bool> IsRunningAsync()
        {
            await _lock.WaitAsync();
            try
            {
                bool result;
                var process = _processFactory.CurrentProcess;

                if (process == null)
                {
                    result = await _webClient.IsMiningAsync();

                    return result;
                }

                if (!process.HasExited)
                {
                    return true;
                }

                result = await _webClient.IsMiningAsync();
                return result;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task Start()
        {
            await _lock.WaitAsync();
            try
            {
                var process = _processFactory.CurrentProcess;

                if (process == null || process.HasExited)
                {
                    _logger.LogInformation("{methodName} {message}", "Start", "Spawning new process because old process " + (process == null ? "is null." : "has exited."));

                    process = await _processFactory.GetNewOrExistingProcessAsync();

                    process.OutputDataReceived += (sender, a) =>
                    {
                        if (!string.IsNullOrEmpty(a.Data))
                            _listener.ReceiveLogMessage(this, new LogMessage() { Message = a.Data });
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    _logger.LogInformation("{methodName} {message}", "Start", "process started, waiting for success status");

                    for (int x = 0; x < _settings.StartupDelayAttempts; x++)
                    {
                        var isRunning = await _webClient.IsRunningAsync();

                        if (isRunning)
                        {
                            _logger.LogInformation("{methodName} {message}", "Start", "Miner has completed starting.");
                            return;
                        }

                        _logger.LogInformation("{methodName} {message}", "Start", "Waiting for miner process to completely start...");
                        await Task.Delay(_settings.StartupDelayInMs);
                    }

                    _logger.LogError("{methodName} {message}", "Start",
                        "Miner did not start after a certain number of attempts.  Try increasing StartupDelayInMs or StartupDelayAttempts in appsettings.json.");
                    return;
                }

                _logger.LogInformation("{methodName} {message}", "Start", "Process already exists.");

                if (!process.HasExited)
                {
                    var isRunning = await _webClient.IsRunningAsync();

                    if (!isRunning)
                    {
                        _logger.LogWarning("{methodName} {message}", "Start", "Miner process is up and has not exited but isn't running yet.  " +
                            "Spawning of the process should have waited for this state to exist.  Is there an earlier error logged here?");
                        return;
                    }

                    var running = await _webClient.IsMiningAsync();

                    if (!running)
                    {
                        _logger.LogInformation("{methodName} {message}", "Start",
                            "Process has not exited and it might be paused, asking existing process to resume.");
                        await _webClient.ResumeAsync();
                    }
                }
            }
            finally
            {
                _lock.Release();
            }
            return;
        }

        public Task Stop()
        {
            _lock.Wait();
            try
            {
                var process = _processFactory.CurrentProcess;

                if (process == null || process.HasExited)
                {
                    _logger.LogDebug("{methodName} {message}", "Stop", "Process has already exited.");
                    return Task.CompletedTask;
                }

                _logger.LogInformation("{methodName} {message}", "Stop", "Killing full process tree.");
                process.Kill(true);
            }
            finally
            {
                _lock.Release();
            }
            return Task.CompletedTask;
        }

        public async Task<bool> TransitionToStateAsync(IMinerState state)
        {
            return await StateHandler.TransitionToStateAsync(state);
        }

        public class MinerStateHandler : IMinerStateHandler
        {
            private IMiner _miner;

            public MinerStateHandler(IMiner miner)
            {
                _miner = miner ?? throw new ArgumentNullException(nameof(miner));
            }

            public event EventHandler<MiningInformation> MiningInformationChanged;

            public virtual void Notify()
            {
                MiningInformationChanged?.Invoke(this, new MiningInformation() { Name = "MiningState", Value = _miner.CurrentState.StateName });
            }

            public virtual async Task<bool> TransitionToStateAsync(IMinerState state)
            {
                if (state == null)
                    throw new ArgumentNullException(nameof(state));

                if (state.Equals(_miner.CurrentState))
                    return false;

                var originalState = _miner.CurrentState;

                await state.EnterStateAsync(_miner);

                if (_miner.CurrentState.Equals(state))
                {
                    Notify();
                    return true;
                }

                if (!_miner.CurrentState.Equals(originalState))
                {
                    //not sure about this.  we could potentially want to transition to a different state than the one
                    //requested.  if state does change, we should notify. 
                    Notify();
                }

                return false;
            }
        }
    }
}
