using System;
using System.Diagnostics;
using System.IO;

namespace DGP.Snap.Framework.Core.Logging
{
    public class Logger
    {
        private readonly bool isLoggingtoFile = true;
        private readonly bool isLoggingtoConsole = true;

        private readonly StreamWriter loggerWriter = new StreamWriter(File.Create("latest.log"));

        internal void LogInternal<T>(object info, Func<object, string> formatter = null)
        {
            if (formatter != null)
                info = formatter.Invoke(info);

            Type type = typeof(T);
            string typename = $"{type.Namespace}.{type.Name}";

            if (this.isLoggingtoFile)
            {
                TextWriter syncWirtter = TextWriter.Synchronized(this.loggerWriter);
                syncWirtter.WriteLine($"{typename}:\n{info}");
            }
            if (this.isLoggingtoConsole)
            {
                Debug.WriteLine($"{typename}:\n{info}");
            }
        }

        #region 单例
        private static Logger instance;
        private static readonly object _lock = new();
        private Logger()
        {
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
        public void UnInitialize()
        {
            //make sure all logs are written in the log file
            this.loggerWriter.Flush();
            this.loggerWriter.Dispose();
        }
    }
}

