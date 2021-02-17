using System.Threading.Tasks;

namespace Collier.Mining
{
    public interface IMiner
    {
        void Stop();
        void Start();
        Task<bool> IsRunningAsync();
    }
}
