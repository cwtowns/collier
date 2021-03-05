using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CollierService.Mining.OutputParsing
{


    public class MinerLogSubject : IObservable<LogMessage>
    {
        public IList<IObserver<LogMessage>> Observers { get; set; }

        public MinerLogSubject()
        {
            Observers = new List<IObserver<LogMessage>>();
        }

        public virtual IDisposable Subscribe(IObserver<LogMessage> observer)
        {
            if (!Observers.Contains(observer))
            {
                Observers.Add(observer);
            }
            return new Unsubscriber(Observers, observer);
        }

        public virtual void SendMessage(string message)
        {
            foreach (var observer in Observers)
            {
                observer.OnNext(new LogMessage { Message = message });
            }
        }
    }

    public class Unsubscriber : IDisposable
    {
        private readonly IObserver<LogMessage> _observer;
        private readonly IList<IObserver<LogMessage>> _observers;
        public Unsubscriber(IList<IObserver<LogMessage>> observers, IObserver<LogMessage> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
            {
                _observers.Remove(_observer);
            }
            GC.SuppressFinalize(this);
        }
    }
}
