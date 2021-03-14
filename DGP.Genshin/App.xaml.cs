using DGP.Genshin.Helpers;
using DGP.Snap.Framework.Core;
using DGP.Snap.Framework.Data.Json;
using System;
using System.IO;
using System.Windows;

namespace DGP.Genshin
{
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            SnapFramework.Instance.UnInitialize();
            base.OnExit(e);
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SnapFramework.Instance.Initialize();
            //app theme
            ThemeHelper.SetAppTheme();
            AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;
        }
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string crashfile = "snap_genshin_crash.log";
            if (!File.Exists(crashfile))
                File.Create(crashfile).Dispose();

            using StreamWriter sw = new(crashfile);
            sw.Write(Json.Stringify(e.ExceptionObject));
        }
    }
}
