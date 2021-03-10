using DGP.Genshin.Services;
using DGP.Genshin.Services.Update;
using DGP.Snap.Framework.Core.LifeCycle;
using DGP.Snap.Framework.Data.Json;
using ModernWpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace DGP.Genshin
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            static ApplicationTheme? converter(object n) => n == null ? null : (ApplicationTheme?)(ApplicationTheme)Enum.Parse(typeof(ApplicationTheme), n.ToString());
            ThemeManager.Current.ApplicationTheme = SettingService.Instance.GetOrDefault(Setting.AppTheme, null, converter);
            LifeCycleManager.Instance.InitializeAll();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            LifeCycleManager.Instance.UnInitializeAll();
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string logFile = "snap_genshin_crash.log";
            if (!File.Exists(logFile))
                File.Create(logFile).Dispose();

            using StreamWriter sw = new StreamWriter(logFile);
            sw.Write(Json.Stringify(e.ExceptionObject));
        }
    }
}
