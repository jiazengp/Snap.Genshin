using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Services;
using DGP.Genshin.Services.Notifications;
using DGP.Genshin.Services.Settings;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace DGP.Genshin
{
    [SuppressMessage("", "CA1001")]
    public partial class App : Application
    {
        private readonly ToastNotificationHandler toastNotificationHandler = new();

        #region LifeCycle
        protected override void OnStartup(StartupEventArgs e)
        {
            EnsureWorkingPath();
            //open main window
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            //handle notification activation
            SetupToastNotificationHandling();
            EnsureSingleInstance();
            //file operation starts
            this.Log($"Snap Genshin - {Assembly.GetExecutingAssembly().GetName().Version}");
            SettingService.Instance.Initialize();
            //app theme
            SetAppTheme();
        }

        private void SetupToastNotificationHandling()
        {
            if (!ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
            {
                //remove toast last time not cleared if it's manually launched
                ToastNotificationManagerCompat.History.Clear();
            }
            ToastNotificationManagerCompat.OnActivated += toastNotificationHandler.OnActivatedByNotification;
        }

        /// <summary>
        /// set working dir while launch by windows autorun
        /// </summary>
        private void EnsureWorkingPath()
        {
            string path = AppContext.BaseDirectory;
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
                SettingService.Instance.UnInitialize();
                MetadataService.Instance.UnInitialize();
                this.Log($"Exit code:{e.ApplicationExitCode}");
            }
            base.OnExit(e);
        }
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!isEnsureingSingleInstance)
            {
                using (StreamWriter sw = new(File.Create($"{DateTime.Now:yyyy-MM-dd HH-mm-ss}-crash.log")))
                {
                    sw.WriteLine($"Snap Genshin - {Assembly.GetExecutingAssembly().GetName().Version}");
                    sw.Write(e.ExceptionObject);
                }
                //while exit with error OnExit will somehow not triggered
            }
        }
        #endregion

        #region SingleInstance
        private const string UniqueEventName = "Snap.Genshin";
        private EventWaitHandle? eventWaitHandle;
        private bool isExitDueToSingleInstanceRestriction;
        private bool isEnsureingSingleInstance;
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
