using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Collier.Host;
using Collier.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Collier.Mining
{
    public interface IMinerProcessFactory
    {
        public Task<IProcess> GetNewOrExistingProcessAsync();
        public IProcess CurrentProcess { get; }
    }

    public class MinerProcessFactory : IMinerProcessFactory
    {
        private ProcessFactory _processFactory;
        private ITrexWebClient _webClient;
        public virtual IProcess CurrentProcess { get; private set; }

        private IApplicationCancellationTokenFactory _cancelFactory;
        private TrexMiner.Settings _minerSettings;
        private TrexWebClient.Settings _webClientSettings;

        private ILogger<MinerProcessFactory> _logger;

        private readonly string _fullyQualfiedMiner;

        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public MinerProcessFactory(ILogger<MinerProcessFactory> logger, IApplicationCancellationTokenFactory cancelFactory, ProcessFactory processFactory, ITrexWebClient webClient,
            IOptions<TrexMiner.Settings> minerSettings, IOptions<TrexWebClient.Settings> webClientSettings)
        {
            _processFactory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
            _webClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            _cancelFactory = cancelFactory ?? throw new ArgumentNullException(nameof(cancelFactory));
            _minerSettings = minerSettings.Value ?? throw new ArgumentNullException(nameof(minerSettings));
            _webClientSettings = webClientSettings.Value ?? throw new ArgumentNullException(nameof(webClientSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _fullyQualfiedMiner = System.IO.Path.Combine(_minerSettings.ExeLocation, _minerSettings.ExeFileName);
        }

        public virtual async Task<IProcess> GetNewOrExistingProcessAsync()
        {
            await _lock.WaitAsync();

            try
            {
                //i might want to check if it's responsive here too
                //if our current process is still running return it
                if (CurrentProcess != null && !CurrentProcess.HasExited)
                {
                    _logger.LogDebug("current process still exists and is running.");
                    return CurrentProcess;
                }


                _logger.LogDebug("current process does not exist, attempting to kill rogue processes");
                //otherwise make sure there are no other processes before we start a new one
                await KillAllRogueProcessesAsync();

                if (_processFactory.GetExistingProcessList(_minerSettings.ExeFileName).Count > 0)
                    throw new Exception("Process still exist after kill commands issued.");

                _logger.LogDebug("spawning new process");
                return CurrentProcess = _processFactory.GetProcess(new ProcessStartInfo
                {
                    FileName = _fullyQualfiedMiner,
                    Arguments = _minerSettings.ExeArguments,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });
            }
            finally
            {
                _lock.Release();
            }
        }


        public virtual async Task KillAllRogueProcessesAsync()
        {
            var hasAttemptedShutdown = false;
            int sleepDuration = _webClientSettings.ShutdownTimeoutMaxMs / _webClientSettings.ShutdownTimeoutNumberOfChecks;

            for (int x = 0; x < _webClientSettings.ShutdownTimeoutNumberOfChecks; x++)
            {
                if (await _webClient.IsRunningAsync())
                {
                    if (!hasAttemptedShutdown)
                    {
                        _logger.LogDebug("attempting graceful web shutdown command");
                        await _webClient.ShutdownAsync();
                        hasAttemptedShutdown = true;
                    }
                }
                else
                {
                    _logger.LogDebug("no web miner running");
                    break;
                }

                await Task.Delay(sleepDuration, _cancelFactory.GetCancellationToken());

                if (_cancelFactory.GetCancellationToken().IsCancellationRequested)
                    return;
            }

            //at this point check for any existing t-rex miners that might be running and kill them
            await KillRemainingMinersAsync();
        }

        public virtual async Task KillRemainingMinersAsync()
        {
            int sleepDuration = _webClientSettings.ShutdownTimeoutMaxMs / _webClientSettings.ShutdownTimeoutNumberOfChecks;

            _logger.LogDebug("manually killing external processes");
            KillAllProcesses();

            await Task.Delay(sleepDuration, _cancelFactory.GetCancellationToken());
        }

        private void KillAllProcesses()
        {
            foreach (var process in _processFactory.GetExistingProcessList(_minerSettings.ExeFileName))
            {
                try
                {
                    _logger.LogDebug("attempting to manually kill pid {processId}", process.Id);
                    process.Kill(true);

                }
                catch (Exception)
                {
                    //no-op 
                }
            }
        }
    }
}