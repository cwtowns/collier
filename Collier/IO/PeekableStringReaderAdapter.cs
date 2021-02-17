using System;
using System.Collections.Generic;
using System.IO;

namespace MiningAutomater.IO
{
    public class PeekableStringReaderAdapter : IDisposable
    {
        private readonly StringReader Underlying;
        private readonly Queue<string> BufferedLines;

        public PeekableStringReaderAdapter(StringReader underlying)
        {
            Underlying = underlying;
            BufferedLines = new Queue<string>();
        }

        public virtual void Dispose()
        {
            Underlying.Dispose();
        }

        public virtual string PeekLine()
        {
            string line = Underlying.ReadLine();
            if (line == null)
                return null;
            BufferedLines.Enqueue(line);
            return line;
        }


        public virtual string ReadLine()
        {
            if (BufferedLines.Count > 0)
                return BufferedLines.Dequeue();
            return Underlying.ReadLine();
        }
    }
}
