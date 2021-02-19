using System.Threading.Tasks;

namespace Collier.IO
{
    public interface IProcess
    {
        bool HasExited { get; }
        int ExitCode { get; }

        void Kill(bool entireProcessTree);
        bool Start();
        void BeginOutputReadLine();

        event DataReceivedEventHandler OutputDataReceived;
        delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);

        Task WaitForExitAsync();

        public int Id { get; }
    }

    public class DataReceivedEventArgs
    {
        public DataReceivedEventArgs(string d)
        {
            Data = d;
        }
        public string Data { get; }
    }
}
