using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Collier.Monitoring.Gpu
{
    public class GpuMonitor : IGpuMonitor
    {
        //C:\Windows\System32\nvidia-smi.exe
        //for usage there are a couple things toi look for.
        //I could check utilization rating, and I can check the active process list for somethign in steam.
        //gpu load seems sketch because i could have brief load, i need to look for sustained load.
        //for example, I checked the gpu load randomly and it was at 53% but no programs were running.  then seconds later it was back under 20%.


        private readonly IEnumerable<IGpuMonitorOutputParser> _outputParsers;
        private readonly ILogger<GpuMonitor> _logger;
        private readonly INvidiaSmiExecutor _smiExecutor;

        public GpuMonitor(ILogger<GpuMonitor> logger, IEnumerable<IGpuMonitorOutputParser> outputParsers, INvidiaSmiExecutor smiExecutor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _outputParsers = outputParsers ?? throw new ArgumentNullException(nameof(outputParsers));
            _smiExecutor = smiExecutor ?? throw new ArgumentNullException(nameof(smiExecutor));
        }

        public virtual async Task<bool> IsGpuUnderLoadAsync()
        {
            var commandOutput = await _smiExecutor.ExecuteCommandAsync();

            if (_smiExecutor.HasErrored)
                throw new ArgumentOutOfRangeException("not sure what this should be");

            foreach (var parser in _outputParsers)
            {
                if (parser.IsGpuUnderLoad(commandOutput))
                    return true;
            }

            return false;
        }
    }
}
