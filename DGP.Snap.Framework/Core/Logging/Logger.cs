using System;
using System.Diagnostics;
using System.IO;

namespace DGP.Snap.Framework.Core.Logging
{
    public class Logger
    {
        private readonly bool isLoggingtoFile = true;

        private readonly StreamWriter loggerWriter = new StreamWriter(File.Create("latest.log"));

        internal void LogInternal<T>(object info, Func<object, string>? formatter = null)
        {
            LogInternal(typeof(T), info, formatter);
        }

        private void LogInternal(Type t, object info, Func<object, string>? formatter = null)
        {
            if (formatter != null)
            {
                info = formatter.Invoke(info);
            }

            Type type = t;
            string typename = $"{type.Namespace}.{type.Name}";
            typename = ToSimplifiedName(typename);
            if (isLoggingtoFile)
            {
                TextWriter syncWirtter = TextWriter.Synchronized(loggerWriter);
                try
                {
                    syncWirtter.WriteLine($"{typename}:\n{info}");
                }
                catch { }
            }
            Debug.WriteLine($"{typename}:{info}");
        }

        public static void LogStatic(Type t, object info, Func<object, string>? formatter = null)
        {
            Instance.LogInternal(t, info, formatter);
        }

        private string ToSimplifiedName(string typename)
        {
            string[] names = typename.Split('.');
            for (int i = 0; i < names.Length - 1; i++)
            {
                names[i] = names[i][0].ToString();
            }
            typename = string.Join(".", names);
            return typename;
        }

        #region 单例
        private static Logger? instance;
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
            loggerWriter.Flush();
            loggerWriter.Dispose();
        }
    }
}

