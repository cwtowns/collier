using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollierService.Hubs
{
    public interface ICollierClient
    {
        Task UpdateMinerLogMessage(string message);
        Task UpdateHashRate(string newRate);
        Task UpdateTemperature(string temp);
        Task UpdateCrashInfo(string temp);

        Task UpdatePower(string power);

        Task UpdateState(string state); /// this will be something different
    }
}
