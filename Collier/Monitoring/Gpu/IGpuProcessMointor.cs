using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollierService.Monitoring.Gpu
{
    public interface IGpuProcessMonitor<T>
    {
        public event EventHandler<T> GpuActivityNoticed;

        public void CheckGpuOutput(string output);
    }

    public class GpuProcessEvent
    {
        public GpuProcessEvent(IList<string> processList)
        {
            ActiveProcesses = new List<string>(processList);
        }

        public IList<string> ActiveProcesses
        {
            get;
        }

    }
}
