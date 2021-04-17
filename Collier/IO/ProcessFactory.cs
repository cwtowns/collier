using System.Collections.Generic;
using System.Diagnostics;

namespace Collier.IO
{
    /// <summary>
    /// Helper class to make testing process based work easier
    /// </summary>
    public class ProcessFactory
    {
        public virtual IProcess GetProcess(ProcessStartInfo startInfo)
        {
            return new ProcessWrapper(new Process
            {
                StartInfo = startInfo
            });
        }

        public virtual IList<IProcess> GetExistingProcessList(string processName)
        {
            var results = new List<IProcess>();
            Process[] processCollection = Process.GetProcessesByName(processName);
            foreach (Process p in processCollection)
            {
                results.Add(new ProcessWrapper(p));
            }

            return results;
        }
    }
}
