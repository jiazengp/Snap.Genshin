using DGP.Genshin.Services;
using DGP.Snap.Framework.Core;
using DGP.Snap.Framework.Core.LifeCycling;
using DGP.Snap.Framework.Data.Json;
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
        protected override void OnExit(ExitEventArgs e)
        {
            this.Log($"Exit with code:{e.ApplicationExitCode}");
            //this service need to uninitialize when exit but not initialize on startup
            RecordService.Instance.UnInitialize();
            SnapFramework.Current.UnInitialize();
            base.OnExit(e);
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;
            SnapFramework.Current.Initialize();
            this.Log($"Snap Genshin - {Assembly.GetExecutingAssembly().GetName().Version}");
            //app theme
            SetAppTheme();
        }
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            this.Log(e.ExceptionObject);
            using StreamWriter sw = new(File.Create("snap_genshin_crash.log"));
            sw.Write(Json.Stringify(e.ExceptionObject));
            SnapFramework.Current.UnInitialize();
        }
        internal static void SetAppTheme()
        {
            static ApplicationTheme? converter(object n) => n == null ? null : (ApplicationTheme)Enum.Parse(typeof(ApplicationTheme), n.ToString());
            ThemeManager.Current.ApplicationTheme = LifeCycle.InstanceOf<SettingService>().GetOrDefault(Setting.AppTheme, null, converter);
        }
    }
}
