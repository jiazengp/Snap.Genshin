using DGP.Genshin.Control;
using DGP.Genshin.Core;
using DGP.Genshin.Core.Notification;
using DGP.Genshin.Core.Plugins;
using DGP.Genshin.Helper;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.Request;
using DGP.Genshin.Service.Abstraction;
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
using System.Reflection;
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

        internal ServiceManagerBase ServiceManager
        {
            get => serviceManager;
        }

        internal IPluginService PluginService
        {
            get => pluginService;
        }

        public TaskbarIcon? NotifyIcon { get; set; }

        public App()
        {
            //prevent later call change executing assembly
            _ = Version;
            pluginService = new PluginService();
            serviceManager = new SnapGenshinServiceManager();
        }

        #region Properties

        /// <summary>
        /// 覆盖默认类型的 Current
        /// </summary>
        public static new App Current
        {
            get => (App)Application.Current;
        }

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

        #region Version
        private Version? version;
        public Version Version
        {
            get
            {
                version ??= Assembly.GetExecutingAssembly().GetName().Version!;
                return version;
            }
        }
        #endregion

        #region IsLaunchedByUser
        public static bool IsLaunchedByUser { get; set; } = true;
        #endregion

        #endregion

        #region Dependency Injection Helper
        /// <summary>
        /// 全局消息交换器
        /// </summary>
        public static WeakReferenceMessenger Messenger
        {
            get => WeakReferenceMessenger.Default;
        }

        internal static object AutoWired(Type type)
        {
            return Current.serviceManager.Services!.GetService(type)
                ?? throw new SnapGenshinInternalException($"无法找到 {type} 类型的对象。");
        }

        /// <summary>
        /// 获取注入的类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="SnapGenshinInternalException">对应的服务类型未注册</exception>
        internal static T AutoWired<T>()
        {
            return Current.serviceManager.Services!.GetService<T>()
                ?? throw new SnapGenshinInternalException($"无法找到 {typeof(T)} 类型的对象。");
        }

        /// <summary>
        /// 查找 <see cref="App.Current.Windows"/> 集合中的对应 <typeparamref name="TWindow"/> 类型的 Window
        /// </summary>
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
            singleInstanceChecker.Ensure(Current, () => BringWindowToFront<MainWindow>());
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
                //make sure settings are saved last
                AutoWired<ISettingService>().UnInitialize();
                try { ToastNotificationManagerCompat.History.Clear(); } catch { }
                this.Log($"Exit code : {e.ApplicationExitCode}");
            }
            base.OnExit(e);
        }
        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            e.Cancel = true;
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
                IsLaunchedByUser = (Environment.CurrentDirectory == workingPath);
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
            ThemeManager.Current.ActualAccentColorChanged += OnActualAccentColorChanged;
            ThemeManager.Current.ApplicationTheme = Setting2.AppTheme.Get();
            //set app accent color to correct color.
            WPFUI.Theme.Manager.ChangeAccentColor(ThemeManager.Current.ActualAccentColor, WPFUI.Theme.Style.Unknown);
        }
        private void OnActualAccentColorChanged(ThemeManager sender, object e)
        {
            //only notify once
            ThemeManager.Current.ActualAccentColorChanged -= OnActualAccentColorChanged;

            new ToastContentBuilder()
                .AddText("检测到系统强调色已更改")
                .AddText("重启程序以正常显示颜色")
                .SafeShow();
        }
        #endregion
    }
}
