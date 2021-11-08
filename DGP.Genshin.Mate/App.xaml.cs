using DGP.Genshin.Mate.Shell;
using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace DGP.Genshin.Mate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            EnsureWorkingPath();
            base.OnStartup(e);
            EnsureSingleInstance();
            //initialize task bar icon
            _ = TaskbarIconManager.Instance;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            TaskbarIconManager.Instance.Dispose();
            base.OnExit(e);
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

        #region SingleInstance
        private const string UniqueEventName = "Snap.Genshin.Mate";
        private EventWaitHandle? eventWaitHandle;
        private void EnsureSingleInstance()
        {
            // check if it is already open.
            try
            {
                // try to open it - if another instance is running, it will exist , if not it will throw
                eventWaitHandle = EventWaitHandle.OpenExisting(UniqueEventName);
                // Notify other instance so it could bring itself to foreground.
                eventWaitHandle.Set();
                // Terminate this instance.
                Shutdown();
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                // listen to a new event (this app instance will be the new "master")
                eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);
            }
        }
        #endregion
    }
}