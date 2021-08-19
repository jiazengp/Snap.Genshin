using DGP.Genshin.Services;
using DGP.Genshin.Services.Settings;
using DGP.Snap.Framework.Core.Logging;
using DGP.Snap.Framework.Extensions.System;
using ModernWpf;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Genshin
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            EnsureSingleInstance();
            //file operation starts
            SettingService.Instance.Initialize();
            this.Log($"Snap Genshin - {Assembly.GetExecutingAssembly().GetName().Version}");
            //app theme
            SetAppTheme();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            if (!this.isExitDueToSingleInstanceRestriction)
            {
                DataService.Instance.UnInitialize();
                SettingService.Instance.UnInitialize();
                this.Log($"Exit code:{e.ApplicationExitCode}");
                Logger.Instance.UnInitialize();
            }
            base.OnExit(e);
        }
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!this.isEnsureingSingleInstance)
            {
                using (StreamWriter sw = new(File.Create($"{DateTime.Now:yyyy-MM-dd HH-mm-ss}-crash.log")))
                {
                    sw.Write(e.ExceptionObject);
                }
            }
        }
        internal void SetAppTheme()
        {
            ThemeManager.Current.ApplicationTheme =
                SettingService.Instance.GetOrDefault(Setting.AppTheme, null, Setting.ApplicationThemeConverter);
        }

        #region SingleInstance
        private const string UniqueEventName = "Snap.Genshin";
        private EventWaitHandle eventWaitHandle;
        private bool isExitDueToSingleInstanceRestriction = false;
        private bool isEnsureingSingleInstance = false;
        private void EnsureSingleInstance()
        {
            // check if it is already open.
            try
            {
                this.isEnsureingSingleInstance = true;
                // try to open it - if another instance is running, it will exist , if not it will throw
                this.eventWaitHandle = EventWaitHandle.OpenExisting(UniqueEventName);
                // Notify other instance so it could bring itself to foreground.
                this.eventWaitHandle.Set();
                // Terminate this instance.
                this.isExitDueToSingleInstanceRestriction = true;
                this.isEnsureingSingleInstance = false;
                Shutdown();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // listen to a new event (this app instance will be the new "master")
                this.eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }
            // if this instance gets the signal to show the main window
            new Task(() =>
            {
                while (this.eventWaitHandle.WaitOne())
                {
                    Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        // could be set or removed anytime
                        if (!Current.MainWindow.Equals(null))
                        {
                            Window mainWindow = Current.MainWindow;
                            if (mainWindow.WindowState == WindowState.Minimized || mainWindow.Visibility != Visibility.Visible)
                            {
                                mainWindow.Show();
                                mainWindow.WindowState = WindowState.Normal;
                            }
                            mainWindow.Activate();
                            mainWindow.Topmost = true;
                            mainWindow.Topmost = false;
                            mainWindow.Focus();
                        }
                    }));
                }
            })
            .Start();
        }
        #endregion
    }
}
