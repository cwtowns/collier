using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Collier.Hubs
{
    public interface ICollierClient
    {
        Task UpdateMinerLogMessage(string message);
        Task UpdateHashRate(string newRate);
        Task UpdateTemperature(string temp);
        Task UpdateCrashInfo(string temp);

        Task UpdatePower(string power);

        Task UpdateState(string state);

        //SignalR clients do not have a listener handler for connection events.
        //you can chain off the start connection event, but that's cubmersome if we
        //want objects farther down the chain to act on connection.  
        //we either have to build it for all clients in JS (which I did in a branch as a 
        //learning exercise) or we send the event from the server
        Task Connected();
    }
}
