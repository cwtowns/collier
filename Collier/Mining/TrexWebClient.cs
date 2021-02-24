using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using CollierService.Monitoring.Gpu;
using Microsoft.Extensions.Logging;

namespace Collier.Mining
{
    public class TrexWebClient : ITrexWebClient
    {
        public class Settings
        {
            public string StatusUrl { get; set; }
            public string PauseUrl { get; set; }
            public string ResumeUrl { get; set; }
            public string ShutdownUrl { get; set; }
            public int ShutdownTimeoutMaxMs { get; set; }

            public int ShutdownTimeoutNumberOfChecks
            {
                get => Math.Max(_shutdownTimeoutNumberOfChecks, 1);

                set => _shutdownTimeoutNumberOfChecks = value;
            }

            private int _shutdownTimeoutNumberOfChecks;
        }

        private readonly HttpClient _httpClient;
        private readonly Settings _settings;
        private readonly ILogger<TrexWebClient> _logger;

        public TrexWebClient(ILogger<TrexWebClient> logger, IOptions<Settings> settings, HttpClient httpClient)
        {

            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual async Task<bool> IsMiningAsync()
        {
            string result;

            try
            {
                result = await _httpClient.GetStringAsync(_settings.StatusUrl);
            }
            catch (Exception e)
            {
                if (LogConnectionRefusedException(e, "PauseAsync"))
                    return false;
                _logger.LogError(e, "IsMiningAsync:  ");
                return false;
            }

            var jsonObject = JObject.Parse(result);

            return (jsonObject.Value<int>("hashrate") > 0);
        }

        public virtual async Task PauseAsync()
        {
            try
            {
                await _httpClient.GetStringAsync(_settings.PauseUrl);
            }
            catch (Exception e)
            {
                if (LogConnectionRefusedException(e, "PauseAsync"))
                    return;
                _logger.LogError(e, "PauseAsync:  ");
            }
        }

        public virtual async Task ResumeAsync()
        {
            try
            {
                await _httpClient.GetStringAsync(_settings.ResumeUrl);
            }
            catch (Exception e)
            {
                if (LogConnectionRefusedException(e, "ResumeAsync"))
                    return;
                _logger.LogError(e, "ResumeAsync:  ");
            }
        }

        public virtual async Task ShutdownAsync()
        {
            try
            {
                await _httpClient.GetStringAsync(_settings.ShutdownUrl);
            }
            catch (Exception e)
            {
                if (LogConnectionRefusedException(e, "ShutdownAsync"))
                    return;
                _logger.LogError(e, "ShutdownAsync:  ");
            }
        }

        public virtual async Task<bool> IsRunningAsync()
        {
            try
            {
                var result = await _httpClient.GetAsync(_settings.StatusUrl);
                return result.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                {
                    if (LogConnectionRefusedException(e, "IsRunningAsync"))
                        return false;
                    _logger.LogError(e, "IsRunningAsync:  ");
                }

                return false;
            }
        }
#nullable enable
        private bool LogConnectionRefusedException(Exception e, string? callingMethod)
        {
            if (e is HttpRequestException re && re?.InnerException is System.Net.Sockets.SocketException)
            {
                //i used to check for the socket error code but oddly could not successfully mock this in GitHub
                _logger.LogDebug("Exception from " + callingMethod + ".  Connection refused.  Miner should not be running.");
                return true;
            }
            return false;
        }
#nullable disable
    }
}
