using DGP.Genshin.DataViewer.Helpers;
using System;
using System.IO;
using System.Windows;

namespace DGP.Genshin.DataViewer
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string logFile = "crash.log";
            if (!File.Exists(logFile))
                File.Create(logFile).Dispose();

            using (StreamWriter sw = new StreamWriter(logFile))
            {
                sw.Write(Json.Stringify(e.ExceptionObject));
            }
        }
    }
}
