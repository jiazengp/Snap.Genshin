using DGP.Genshin.Helpers;
using DGP.Snap.Framework.Core;
using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System;
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
            ThemeHelper.SetAppTheme();
            
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
