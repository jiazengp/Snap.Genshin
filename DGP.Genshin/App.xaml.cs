using DGP.Genshin.Services;
using DGP.Genshin.Services.Settings;
using DGP.Genshin.Common.Core.Logging;
using DGP.Genshin.Common.Extensions.System;
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
        #region LifeCycle
        protected override void OnStartup(StartupEventArgs e)
        {
            EnsureWorkingPath();
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            EnsureSingleInstance();
            //file operation starts
            this.Log($"Snap Genshin - {Assembly.GetExecutingAssembly().GetName().Version}");
            SettingService.Instance.Initialize();
            //app theme
            SetAppTheme();
            //post app window created

        }
        private void EnsureWorkingPath()
        {
            //set working dir while launch by windows autorun
            string? path = Assembly.GetEntryAssembly()?.Location;
            string? workingPath = Path.GetDirectoryName(path);
            if (workingPath is not null)
            {
                Environment.CurrentDirectory = workingPath;
            }

        }
        private void SetAppTheme()
        {
            ThemeManager.Current.ApplicationTheme =
                SettingService.Instance.GetOrDefault(Setting.AppTheme, null, Setting.ApplicationThemeConverter);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            if (!isExitDueToSingleInstanceRestriction)
            {
                MetaDataService.Instance.UnInitialize();
                SettingService.Instance.UnInitialize();
                this.Log($"Exit code:{e.ApplicationExitCode}");
                Logger.Instance.UnInitialize();
            }
            base.OnExit(e);
        }
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!isEnsureingSingleInstance)
            {
                using (StreamWriter sw = new(File.Create($"{DateTime.Now:yyyy-MM-dd HH-mm-ss}-crash.log")))
                {
                    sw.Write($"Snap Genshin - {Assembly.GetExecutingAssembly().GetName().Version}");
                    sw.Write(e.ExceptionObject);
                }
                Logger.Instance.UnInitialize();
            }
        }
        #endregion

        #region SingleInstance
        private const string UniqueEventName = "Snap.Genshin";
        private EventWaitHandle? eventWaitHandle;
        private bool isExitDueToSingleInstanceRestriction = false;
        private bool isEnsureingSingleInstance = false;
        private void EnsureSingleInstance()
        {
            // check if it is already open.
            try
            {
                isEnsureingSingleInstance = true;
                // try to open it - if another instance is running, it will exist , if not it will throw
                eventWaitHandle = EventWaitHandle.OpenExisting(UniqueEventName);
                // Notify other instance so it could bring itself to foreground.
                eventWaitHandle.Set();
                // Terminate this instance.
                isExitDueToSingleInstanceRestriction = true;
                Shutdown();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                isEnsureingSingleInstance = false;
                // listen to a new event (this app instance will be the new "master")
                eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }
            // if this instance gets the signal to show the main window
            new Task(() =>
            {
                while (eventWaitHandle.WaitOne())
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
