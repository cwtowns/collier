using Newtonsoft.Json.Linq;

namespace MiningAutomater.Monitoring.Gpu
{
    public interface INvidiaSmiParser
    {
        JObject Parse(string data);
    }
}