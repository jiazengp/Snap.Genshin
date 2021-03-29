using DGP.Snap.Framework.Core.Entry;
using DGP.Snap.Framework.Core.LifeCycling;
using System;
using System.Diagnostics;

namespace DGP.Snap.Framework.Core.Logging
{
    internal class Logger
    {
        public Logger()
        {
            Initialize();
        }
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
                var type = obj.GetType();
                string typename = $"{type.Namespace}.{type.Name}";
                Debug.WriteLine($"{DateTime.Now} | DEBUG | {typename.PadRight(maxTypeLength)}:{info}");
            }
        }

        public void Initialize()
        {
            foreach (Type type in EntryHelper.GetCurrentTypes())
            {
                string typename = $"{type.Namespace}.{type.Name}";
                Debug.WriteLine($"{DateTime.Now} | DEBUG | {typename.PadRight(maxTypeLength)}:");
                int typeLength = typename.Length;
                if (typeLength > this.maxTypeLength)
                    this.maxTypeLength = typeLength;
                
            }
        }

        public void UnInitialize()
        {

        }
    }
}

