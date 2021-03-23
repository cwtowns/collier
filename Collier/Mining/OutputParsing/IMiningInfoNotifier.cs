using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Collier.Mining.OutputParsing
{
    public interface IMiningInfoNotifier
    {
        event EventHandler<MiningInformation> MiningInformationChanged;
        void Notify();
    }

    public class MiningInformation
    {
        public virtual string Name { get; set; }
        public virtual string Value { get; set; }
    }
}
