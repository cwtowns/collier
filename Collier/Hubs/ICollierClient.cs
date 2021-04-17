using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Collier.Hubs
{
    /// <summary>
    /// Represents what methods we can call on the client from the server.  I ended up doing most
    /// event notification generically in SignalRBackgroundService so this interface didn't get built
    /// out as much as planned.  
    /// </summary>
    public interface ICollierClient
    {
        //SignalR clients do not have a listener handler for connection events.
        //you can chain off the start connection event, but that's cubmersome if we
        //want objects farther down the chain to act on connection.  
        //we either have to build it for all clients in JS (which I did in a branch as a 
        //learning exercise) or we send the event from the server
        Task Connected();
    }
}
