using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollierService.Mining.OutputParsing
{
    public class ExternalLogObserver : IObserver<LogMessage>
    {
        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(LogMessage value)
        {
            throw new NotImplementedException();
        }
    }
}
