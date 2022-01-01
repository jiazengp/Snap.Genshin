using DGP.Genshin.Common.Exceptions;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Common.Request;
using DGP.Genshin.Controls;
using DGP.Genshin.Core;
using DGP.Genshin.Core.Plugins;
using DGP.Genshin.Helpers;
using DGP.Genshin.Helpers.Notifications;
using DGP.Genshin.Services.Abstratcions;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            serviceManager = new PluginSupportedServiceManager();
        }

        /// <summary>
        /// 覆盖默认类型的 Current
        /// </summary>
        public new static App Current => (App)Application.Current;

        #region Dependency Injection Helper
        /// <summary>
        /// 全局消息交换器
        /// </summary>
        public static WeakReferenceMessenger Messenger => WeakReferenceMessenger.Default;

        /// <summary>
        /// 获取应注入的服务
        /// 获取时应使用服务的接口类型
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        /// <exception cref="SnapGenshinInternalException">对应的服务类型未注册</exception>
        public static TService GetService<TService>()
        {
            return Current.serviceManager.Services!.GetService<TService>()
                ?? throw new SnapGenshinInternalException($"无法找到 {typeof(TService)} 类型的服务");
        }

        /// <summary>
        /// 获取应注入的视图模型
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <returns></returns>
        public static TViewModel GetViewModel<TViewModel>()
        {
            return GetService<TViewModel>();
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
            }
        }

        /// <summary>
        /// 查找 <see cref="Application.Current.Windows"/> 集合中的对应 <typeparamref name="TWindow"/> 类型的 Window
        /// </summary>
        /// <returns>返回唯一的窗口，未找到返回新实例</returns>
        public static void BringWindowToFront<TWindow>(string? name = null) where TWindow : Window, new()
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
            this.Log($"Snap Genshin - {Assembly.GetExecutingAssembly().GetName().Version}");
            GetService<ISettingService>().Initialize();
            //app theme
            UpdateAppTheme();
            //app center services
            ConfigureAppCenter(true);
            //global requester callback
            ConfigureRequester();
            //open main window
            base.OnStartup(e);
        }
        protected override void OnExit(ExitEventArgs e)
        {
            if (!singleInstanceChecker.IsExitDueToSingleInstanceRestriction)
            {
                GetService<ISettingService>().UnInitialize();
                ToastNotificationManagerCompat.History.Clear();
                this.Log($"Exit code : {e.ApplicationExitCode}");
            }
            base.OnExit(e);
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
            Requester.ResponseFailedCallback = (ex, method, desc) =>
            Crashes.TrackError(ex, new Dictionary<string, string> { { method, desc } });
        }
        private void ConfigureWorkingDirectory()
        {
            if (Path.GetDirectoryName(AppContext.BaseDirectory) is string workingPath)
            {
                Environment.CurrentDirectory = workingPath;
            }
        }
        private void ConfigureAppCenter(bool enabled)
        {
            if (enabled)
            {
                AppCenter.SetUserId(User.Id);
                AppCenter.LogLevel = LogLevel.Verbose;
                //cause the version of debug is always higher than normal release
                //we need to send debug info to separate kanban
#if DEBUG
                //DEBUG INFO should send to Snap Genshin Debug kanban
                AppCenter.Start("2e4fa440-132e-42a7-a288-22ab1a8606ef", typeof(Analytics), typeof(Crashes));
#else
                //RELEASE INFO should send to Snap Genshin kanban
                AppCenter.Start("031f6319-175f-475a-a2a6-6e13eaf9bb08", typeof(Analytics), typeof(Crashes));
#endif
            }
        }
        private void ConfigureToastNotification()
        {
            ToastNotificationManagerCompat.OnActivated += toastNotificationHandler.OnActivatedByNotification;
        }
        private void UpdateAppTheme()
        {
            ThemeManager.Current.ApplicationTheme =
                GetService<ISettingService>().GetOrDefault(Setting.AppTheme, null, Setting.ApplicationThemeConverter);
        }

        #endregion
    }
}
