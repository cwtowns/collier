using Collier.Mining.OutputParsing;
using System;
using System.Threading.Tasks;

namespace Collier.Mining
{
    public interface IMiner
    {
        Task Stop();
        Task Start();
        Task<bool> IsRunningAsync();

        enum MiningState { Unknown, Running, Stopped, Paused };
    }
}
