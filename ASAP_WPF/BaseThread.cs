using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ASAP_WPF
{
    public abstract class BaseThread
    {
        private readonly Thread _thread;

        protected BaseThread()
        {
            _thread = new Thread(new ThreadStart(this.RunThread));
        }

        // Thread methods / properties
        public void Start() => _thread.Start();
        public void Join() => _thread.Join();
        public bool IsAlive => _thread.IsAlive;

        // Override in base class
        public abstract void RunThread();
    }
}
