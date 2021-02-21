using System;
using System.Linq;
using System.Threading.Tasks;

namespace CollierService.Monitoring.Gpu
{
    public interface IGpuProcessMonitor<T>
    {
        public event EventHandler<T> GpuActivityNoticed;

        public void CheckGpuOutput(string output);
    }
}
