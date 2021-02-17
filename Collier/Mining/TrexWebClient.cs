using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MiningAutomater.Mining
{
    public class TrexWebClient : ITrexWebClient
    {
        public class Settings
        {
            public string StatusUrl { get; set; }
            public string PauseUrl { get; set; }
            public string ResumeUrl { get; set; }
        }

        private readonly HttpClient _httpClient;
        private readonly Settings _settings;

        public TrexWebClient(IOptions<Settings> settings, HttpClient httpClient)
        {

            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public async virtual Task<bool> IsRunningAsync()
        {
            var result = await _httpClient.GetStringAsync(_settings.StatusUrl);

            var jsonObject = JObject.Parse(result);

            return (jsonObject.Value<int>("hashrate") > 0);
        }

        public async virtual void PauseAsync()
        {
            await _httpClient.GetStringAsync(_settings.PauseUrl);
        }

        public async virtual void ResumeAsync()
        {
            await _httpClient.GetStringAsync(_settings.ResumeUrl);
        }
    }
}
