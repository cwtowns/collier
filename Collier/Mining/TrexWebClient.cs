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

        public async virtual Task<bool> IsMiningAsync()
        {
            string result = string.Empty;

            try
            {
                result = await _httpClient.GetStringAsync(_settings.StatusUrl);
            }
            catch (HttpRequestException re)
            {
                _logger.LogError(re, "IsMiningAsync:  ");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "IsMiningAsync:  ");
                throw;
            }

            var jsonObject = JObject.Parse(result);

            return (jsonObject.Value<int>("hashrate") > 0);
        }

        public async virtual Task PauseAsync()
        {
            try
            {
                await _httpClient.GetStringAsync(_settings.PauseUrl);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "PauseAsync:  ");
            }
        }

        public async virtual Task ResumeAsync()
        {
            try
            {
                await _httpClient.GetStringAsync(_settings.ResumeUrl);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ResumeAsync:  ");
            }
        }

        public async virtual Task ShutdownAsync()
        {
            try
            {
                await _httpClient.GetStringAsync(_settings.ShutdownUrl);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ShutdownAsync:  ");
            }
        }

        public async virtual Task<bool> IsRunningAsync()
        {
            try
            {
                var result = await _httpClient.GetAsync(_settings.StatusUrl);
                return result.IsSuccessStatusCode;
            }
            catch (HttpRequestException re)
            {
                if (re.InnerException is System.Net.Sockets.SocketException)
                {
                    //i used to check for the socket error code but oddly could not successfully mock this in GitHub
                    _logger.LogInformation("Connection refused.  Miner should not be running.");
                        return false;
                }
                _logger.LogInformation(re, "IsMiningAsync:  ");
                throw new ArgumentOutOfRangeException(
                    "this is a response exception but its inner exception is not the right type", re);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "IsRunningAsync:  ");
            }

            return false;
        }
    }
}
