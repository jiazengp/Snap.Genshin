using DGP.Snap.Framework.Core.Entry;
using DGP.Snap.Framework.Core.LifeCycle;
using System;
using System.Diagnostics;

namespace DGP.Snap.Framework.Core.Logging
{
    internal class Logger : ILifeCycleManaged
    {
        private readonly bool isLoggingtoFile = false;
        private readonly bool isLoggingtoConsole = true;

        private int maxTypeLength = 0;

        public void Log(object obj, object info, Func<object, string> formatter = null)
        {
            if (formatter != null)
                info = formatter.Invoke(info);

            if (this.isLoggingtoFile)
            {

            }
            if (this.isLoggingtoConsole)
            {
                Debug.WriteLine("length:" + maxTypeLength);
                Debug.WriteLine($"{DateTime.Now} | DEBUG | {obj.ToString().PadLeft(maxTypeLength)}:{info}");
            }
        }

        public void Initialize()
        {
            foreach (Type type in EntryHelper.GetCurrentTypes())
            {
                int typeLength = type.ToString().Length;
                if (typeLength > this.maxTypeLength)
                    this.maxTypeLength = typeLength;
            }
            Debug.WriteLine("length:" + maxTypeLength);
        }

        public void UnInitialize()
        {

        }
    }

    public delegate void LogHandler(object info);
}

