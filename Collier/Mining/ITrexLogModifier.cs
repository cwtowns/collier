using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollierService.Mining
{
    public interface ITrexLogModifier
    {
        string ModifyLog(string originalMessage);
    }
}
