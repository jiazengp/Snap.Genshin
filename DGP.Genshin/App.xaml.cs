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
            SnapFramework.Current.UnInitialize();
            base.OnExit(e);
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //unhandled exception
            AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;
            //initialize framework
            SnapFramework.Current.Initialize();
            this.Log(Assembly.GetExecutingAssembly().GetName().Version);
            //app theme
            SetAppTheme();
        }
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            using StreamWriter sw = new(File.Create("snap_genshin_crash.log"));
            sw.Write(Json.Stringify(e.ExceptionObject));
        }
        internal static void SetAppTheme()
        {
            static ApplicationTheme? converter(object n) => n == null ? null : (ApplicationTheme)Enum.Parse(typeof(ApplicationTheme), n.ToString());
            ThemeManager.Current.ApplicationTheme = LifeCycle.InstanceOf<SettingService>().GetOrDefault(Setting.AppTheme, null, converter);
        }
    }
}
