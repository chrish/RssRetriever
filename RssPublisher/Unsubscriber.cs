using System;
using System.Collections.Generic;
using System.Text;

namespace RssPublisher
{
    internal class Unsubscriber<Story> : IDisposable
    {
        private List<IObserver<Story>> _observers;
        private IObserver<Story> _observer;

        internal Unsubscriber(List<IObserver<Story>> observers, IObserver<Story> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose()
        {
            if (_observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}
