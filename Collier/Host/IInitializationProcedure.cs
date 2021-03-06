using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Collier.Host
{
    //TODO The way I'm using this is a code smell, get rid of it somehow.  Maybe my signalr work will eliminate the need here.
    //i really just need it for wiring up the internal logging observer...
    public interface IInitializationProcedure
    {
        Task Init();
    }
}
