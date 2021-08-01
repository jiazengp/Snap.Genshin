using DGP.Snap.Framework.Core.Entry;
using System;
using System.Diagnostics;
using System.IO;

namespace DGP.Snap.Framework.Core.Logging
{
    internal class Logger
    {
        private readonly bool isLoggingtoFile = true;
        private readonly bool isLoggingtoConsole = true;

        private int maxTypeLength = 0;

        private readonly StreamWriter loggerWriter = new StreamWriter(File.Create("latest.log"));

        public void Log(object obj, object info, Func<object, string> formatter = null)
        {
            if (formatter != null)
                info = formatter.Invoke(info);

            Type type = obj.GetType();
            string typename = $"{type.Namespace}.{type.Name}";

            if (this.isLoggingtoFile)
            {
                this.loggerWriter.WriteLine($"{DateTime.Now} | DEBUG | {typename.PadRight(this.maxTypeLength)}:{info}");
            }
            if (this.isLoggingtoConsole)
            {
                Debug.WriteLine($"{DateTime.Now} | DEBUG | {typename.PadRight(this.maxTypeLength)}:{info}");
            }
        }

        #region 单例
        private static Logger instance;
        private static readonly object _lock = new();
        private Logger()
        {
            this.Initialize();
        }
        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new Logger();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion

        public void Initialize()
        {
            foreach (Type type in EntryHelper.GetCurrentTypes())
            {
                string typename = $"{type.Namespace}.{type.Name}";
                int typeLength = typename.Length;
                if (typeLength > this.maxTypeLength)
                    this.maxTypeLength = typeLength;
            }
        }

        public void UnInitialize()
        {
            //make sure all logs are written in the log file
            this.loggerWriter.Flush();
            this.loggerWriter.Dispose();
        }
    }
}

