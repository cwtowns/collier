using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiningAutomater.IO;
using MiningAutomater.Mining;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CollierService.Mining
{
    public class MinerProcessFactory
    {
        private ProcessFactory _processFactory;

        protected virtual IProcess CurrentProcess { get; private set; }

        private ITrexWebClient _trexWebClient;
        private ILogger<TrexMiner> _logger;
        private TrexMiner.Settings _settings;
        private readonly string _fullyQualfiedMiner;

        public MinerProcessFactory(ILogger<TrexMiner> logger, IOptions<TrexMiner.Settings> settings, ProcessFactory processFactory, ITrexWebClient webClient)
        {
            _processFactory = processFactory ?? throw new ArgumentNullException(nameof(processFactory));
            _trexWebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _fullyQualfiedMiner = System.IO.Path.Combine(_settings.ExeLocation, _settings.ExeFileName);

            if (!System.IO.File.Exists(_fullyQualfiedMiner))
                _logger.LogError("miner does not exist at {location}", _fullyQualfiedMiner);
        }

        public virtual async Task<IProcess> GetMinerProcess()
        {
            //we have a process that is already running
            if (CurrentProcess != null && CurrentProcess.HasExited == false)
                return CurrentProcess;

            //if no process exists externally, launch a new one
            if (await _trexWebClient.IsRunningAsync() == false)
            {
                return CurrentProcess = _processFactory.GetProcess(new ProcessStartInfo
                {
                    FileName = _fullyQualfiedMiner,
                    Arguments = _settings.ExeArguments,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                });
            }



            throw new NotImplementedException();
        }
    }
}
