using System.Collections.Generic;

namespace CollierService.Monitoring.Gpu
{
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