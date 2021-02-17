using Newtonsoft.Json.Linq;

namespace Collier.Monitoring.Gpu
{
    public interface INvidiaSmiParser
    {
        JObject Parse(string data);
    }
}