using System.Diagnostics;

namespace Collier.IO
{
    public class ProcessFactory
    {
        public virtual IProcess GetProcess(ProcessStartInfo startInfo)
        {
            return new ProcessWrapper(new Process
            {
                StartInfo = startInfo
            });
        }
    }
}
