using DGP.Genshin.Services;
using DGP.Genshin.Services.Settings;
using DGP.Snap.Framework.Core.Logging;
using DGP.Snap.Framework.Extensions.System;
using ModernWpf;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace DGP.Genshin
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;
            SettingService.Instance.Initialize();
            this.Log($"Snap Genshin - {Assembly.GetExecutingAssembly().GetName().Version}");
            //app theme
            SetAppTheme();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            RecordService.Instance.UnInitialize();
            DataService.Instance.UnInitialize();
            SettingService.Instance.UnInitialize();
            this.Log($"Exit code:{e.ApplicationExitCode}");
            Logger.Instance.UnInitialize();
            base.OnExit(e);
        }
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            using StreamWriter sw = new(File.Create("snap_genshin_crash.log"));
            sw.Write(e.ExceptionObject);
        }
        internal static void SetAppTheme()
        {
            static ApplicationTheme? converter(object n) => n == null ? null : (ApplicationTheme)Enum.Parse(typeof(ApplicationTheme), n.ToString());
            ThemeManager.Current.ApplicationTheme = SettingService.Instance.GetOrDefault(Setting.AppTheme, null, converter);
        }
    }
}
