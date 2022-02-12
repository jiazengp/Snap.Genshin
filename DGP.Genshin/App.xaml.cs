using DGP.Genshin.Control;
using DGP.Genshin.Core;
using DGP.Genshin.Core.Plugins;
using DGP.Genshin.Helper;
using DGP.Genshin.Helper.Notification;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.Request;
using DGP.Genshin.Service.Abstratcion;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf;
using Snap.Core.Logging;
using Snap.Exception;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows;

namespace DGP.Genshin
{
    /// <summary>
    /// Snap Genshin
    /// </summary>
    public partial class App : Application
    {
        private readonly ToastNotificationHandler toastNotificationHandler = new();
        private readonly SingleInstanceChecker singleInstanceChecker = new("Snap.Genshin");
        private readonly ServiceManagerBase serviceManager;
        private readonly IPluginService pluginService;

        internal ServiceManagerBase ServiceManager => serviceManager;
        internal IPluginService PluginService => pluginService;
        public TaskbarIcon? NotifyIcon { get; set; }

        public App()
        {
            pluginService = new PluginService();
            serviceManager = new SnapGenshinServiceManager();
        }

        /// <summary>
        /// 覆盖默认类型的 Current
        /// </summary>
        public static new App Current => (App)Application.Current;

        #region IsElevated
        private static bool? isElevated;
        public static bool IsElevated
        {
            get
            {
                isElevated ??= GetElevated();
                return isElevated.Value;
            }
        }

        private static bool GetElevated()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        #endregion

        #region Dependency Injection Helper
        /// <summary>
        /// 全局消息交换器
        /// </summary>
        public static WeakReferenceMessenger Messenger => WeakReferenceMessenger.Default;

        /// <summary>
        /// 获取注入的类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="SnapGenshinInternalException">对应的服务类型未注册</exception>
        public static T AutoWired<T>()
        {
            return Current.serviceManager.Services!.GetService<T>()
                ?? throw new SnapGenshinInternalException($"无法找到 {typeof(T)} 类型的对象。");
        }

        /// <summary>
        /// 查找 <see cref="Application.Current.Windows"/> 集合中的对应 <typeparamref name="TWindow"/> 类型的 Window
        /// </summary>
        /// <returns>返回唯一的窗口，未找到返回新实例</returns>
        public static void ShowOrCloseWindow<TWindow>(string? name = null) where TWindow : Window, new()
        {
            TWindow? window = Current.Windows.OfType<TWindow>().FirstOrDefault();

            if (window is not null)
            {
                window.Close();
            }
            else
            {
                TWindow newWindow = new();
                newWindow.Show();

                newWindow.Activate();
                newWindow.Topmost = true;
                newWindow.Topmost = false;
                newWindow.Focus();
            }
        }

        /// <summary>
        /// 查找 <see cref="Application.Current.Windows"/> 集合中的对应 <typeparamref name="TWindow"/> 类型的 Window
        /// </summary>
        /// <returns>返回唯一的窗口，未找到返回新实例</returns>
        public static void BringWindowToFront<TWindow>() where TWindow : Window, new()
        {
            TWindow? window = Current.Windows.OfType<TWindow>().FirstOrDefault();

            if (window is null)
            {
                window = new();
            }
            if (window.WindowState == WindowState.Minimized || window.Visibility != Visibility.Visible)
            {
                window.Show();
                window.WindowState = WindowState.Normal;
            }
            window.Activate();
            window.Topmost = true;
            window.Topmost = false;
            window.Focus();
        }
        #endregion

        #region LifeCycle
        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureWorkingDirectory();
            ConfigureUnhandledException();
            //handle notification activation
            ConfigureToastNotification();
            singleInstanceChecker.EnsureAsync(Current, () => BringWindowToFront<MainWindow>());
            //app center services
            ConfigureAppCenter(true);
            //global requester callback
            ConfigureRequester();
            //services
            AutoWired<ISettingService>().Initialize();
            //app theme
            UpdateAppTheme();
            //open main window
            base.OnStartup(e);
            BringWindowToFront<MainWindow>();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            if (!singleInstanceChecker.IsExitDueToSingleInstanceRestriction)
            {
                Messenger.Send(new AppExitingMessage());
                AutoWired<ISettingService>().UnInitialize();
                try { ToastNotificationManagerCompat.History.Clear(); } catch { }
                this.Log($"Exit code : {e.ApplicationExitCode}");
            }
            base.OnExit(e);
        }
        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            if (!singleInstanceChecker.IsExitDueToSingleInstanceRestriction)
            {
                Messenger.Send(new AppExitingMessage());
                AutoWired<ISettingService>().UnInitialize();
                try { ToastNotificationManagerCompat.History.Clear(); } catch { }
            }
            base.OnSessionEnding(e);
        }

        private void ConfigureUnhandledException()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!singleInstanceChecker.IsEnsureingSingleInstance)
            {
                //unhandled exception now can be uploaded automatically
                new ExceptionWindow((Exception)e.ExceptionObject).ShowDialog();
            }
        }
        private void ConfigureRequester()
        {
            Requester.ResponseFailedAction = (ex, method, desc) =>
            Crashes.TrackError(ex, new Dictionary<string, string> { { method, desc } });
        }
        private void ConfigureWorkingDirectory()
        {
            if (Path.GetDirectoryName(AppContext.BaseDirectory) is string workingPath)
            {
                Environment.CurrentDirectory = workingPath;
            }
        }
        partial void ConfigureAppCenter(bool enabled);
        private void ConfigureToastNotification()
        {
            ToastNotificationManagerCompat.OnActivated += toastNotificationHandler.OnActivatedByNotification;
        }
        private void UpdateAppTheme()
        {
            ThemeManager.Current.ApplicationTheme =
                AutoWired<ISettingService>().GetOrDefault(Setting.AppTheme, null, Setting.ApplicationThemeConverter);
            //ThemeManager.Current.AccentColor = ThemeManager.Current.ActualAccentColor;
        }
        #endregion
    }
}
