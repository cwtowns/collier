using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Collier.IO
{
    public class ProcessWrapper : IProcess
    {
        private readonly Process _process;
        public ProcessWrapper(Process process)
        {
            _process = process ?? throw new ArgumentNullException(nameof(process));
            _process.OutputDataReceived += Corehandler;
        }
        public bool HasExited => _process.HasExited;

        public int ExitCode => _process.ExitCode;

        public event IProcess.DataReceivedEventHandler OutputDataReceived;

        private void Corehandler(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            OutputDataReceived?.Invoke(sender, new DataReceivedEventArgs(e.Data));
        }

        public int Id => _process.Id;

        public void BeginOutputReadLine()
        {
            _process.BeginOutputReadLine();

        }

        public void Kill(bool entireProcessTree)
        {
            _process.Kill(entireProcessTree);
        }

        public bool Start()
        {
            return _process.Start();
        }

        public Task WaitForExitAsync()
        {
            return _process.WaitForExitAsync();
        }
    }
}
