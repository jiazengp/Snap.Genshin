using DGP.Snap.Framework.Core.LifeCycle;
using System;
using System.Diagnostics;

namespace DGP.Snap.Framework.Core.Logging
{
    internal class Logger : ILifeCycleManaged
    {
        private readonly bool isLoggingtoFile = false;
        private readonly bool isLoggingtoConsole = true;

        public void Log(object obj, object info, Func<object, string> formatter = null)
        {
            if (formatter != null)
                info = formatter.Invoke(info);
            if (this.isLoggingtoFile)
            {

            }
            if (this.isLoggingtoConsole)
            {

                Debug.WriteLine($"{DateTime.Now}|{obj}:{info}");
            }
        }

        public void Initialize()
        {

        }

        public void UnInitialize()
        {

        }
    }

    public delegate void LogHandler(object info);
}

