using CommunityToolkit.Mvvm.Messaging;
using DGP.Genshin.Control;
using DGP.Genshin.Core;
using DGP.Genshin.Core.ImplementationSwitching;
using DGP.Genshin.Core.LifeCycle;
using DGP.Genshin.Core.Notification;
using DGP.Genshin.Core.Plugins;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.Request;
using DGP.Genshin.Service.Abstraction.Setting;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf;
using Snap.Core.Logging;
using Snap.Extenion.Enumerable;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using WPFUI.Appearance;

namespace DGP.Genshin
{
    /// <summary>
    /// Snap Genshin
    /// </summary>
    public partial class App : Application
    {
        private static bool? isElevated;

        private readonly ToastNotificationHandler toastNotificationHandler = new();
        private readonly SingleInstanceChecker singleInstanceChecker = new("Snap.Genshin");
        private readonly ServiceManagerBase serviceManager;
        private readonly IPluginService pluginService;
        private readonly SwitchableImplementationManager switchableImplementationManager;

        private Version? version;

        /// <summary>
        /// 构造一个新的 Snap Genshin 实例
        /// </summary>
        public App()
        {
            // prevent later call change executing assembly
            _ = Version;
            pluginService = new PluginService();
            switchableImplementationManager = new();
            serviceManager = new SnapGenshinServiceManager();
            switchableImplementationManager.SwitchToCorrectImplementations();
        }

        /// <summary>
        /// 覆盖默认类型的 Current
        /// </summary>
        public static new App Current
        {
            get => (App)Application.Current;
        }

        /// <summary>
        /// 检查当前应用程序是否提权
        /// </summary>
        public static bool IsElevated
        {
            get
            {
                isElevated ??= GetElevated();
                return isElevated.Value;
            }
        }

        /// <summary>
        /// 检查应用程序是否由用户手动启动
        /// </summary>
        public static bool IsLaunchedByUser { get; set; } = true;

        /// <summary>
        /// 全局消息交换器
        /// </summary>
        public static WeakReferenceMessenger Messenger
        {
            get => WeakReferenceMessenger.Default;
        }

        /// <summary>
        /// 当前的版本号
        /// </summary>
        public Version Version
        {
            get
            {
                version ??= Assembly.GetExecutingAssembly().GetName().Version!;
                return version;
            }
        }

        /// <summary>
        /// 任务栏图标
        /// </summary>
        public TaskbarIcon? NotifyIcon { get; set; }

        /// <summary>
        /// 服务管理器
        /// </summary>
        internal ServiceManagerBase ServiceManager
        {
            get => serviceManager;
        }

        /// <summary>
        /// 插件服务
        /// </summary>
        internal IPluginService PluginService
        {
            get => pluginService;
        }

        /// <summary>
        /// 可切换服务实现
        /// </summary>
        internal SwitchableImplementationManager SwitchableImplementationManager
        {
            get => switchableImplementationManager;
        }

        /// <summary>
        /// 用于插件发现注入服务的容器实例
        /// </summary>
        internal Core.IContainer DI { get; } = new DefaultContainter();

        /// <summary>
        /// 以管理员权限重启
        /// </summary>
        public static void RestartAsElevated()
        {
            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    FileName = PathContext.Locate("DGP.Genshin.exe"),
                });
            }
            catch (Win32Exception)
            {
                return;
            }

            Current.Shutdown();
        }

        /// <summary>
        /// 查找 <see cref="App.Current.Windows"/> 集合中的对应 <typeparamref name="TWindow"/> 类型的 Window
        /// 将其唤至前台
        /// </summary>
        /// <typeparam name="TWindow">窗体类型</typeparam>
        public static void BringWindowToFront<TWindow>()
            where TWindow : Window, new()
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

        /// <summary>
        /// 获取注入的类型
        /// </summary>
        /// <param name="type">服务的类型</param>
        /// <returns>服务</returns>
        internal static object AutoWired(Type type)
        {
            object? service = Current.serviceManager.Services!.GetService(type);
            return Must.NotNull(service!);
        }

        /// <summary>
        /// 获取注入的类型
        /// </summary>
        /// <typeparam name="T">服务的类型</typeparam>
        /// <returns>服务</returns>
        internal static T AutoWired<T>()
            where T : class
        {
            T? service = Current.serviceManager.Services!.GetService<T>();
            return Must.NotNull(service!);
        }

        /// <inheritdoc/>
        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureWorkingDirectory();
            ConfigureUnhandledException();

            // handle notification activation
            ConfigureToastNotification();
            singleInstanceChecker.Ensure(Current, () => BringWindowToFront<MainWindow>());

            // app center services
            ConfigureAppCenter(true);

            // global requester callback
            ConfigureRequester();

            // services
            AutoWired<ISettingService>().Initialize();

            // app theme
            UpdateAppTheme();
            TriggerAppStartUpEvent();

            // open main window
            base.OnStartup(e);
            BringWindowToFront<MainWindow>();
        }

        /// <inheritdoc/>
        protected override void OnExit(ExitEventArgs e)
        {
            if (!singleInstanceChecker.IsExitDueToSingleInstanceRestriction)
            {
                Messenger.Send(new AppExitingMessage());
                switchableImplementationManager.UnInitialize();

                // make sure settings are saved last
                AutoWired<ISettingService>().UnInitialize();
                try
                {
                    ToastNotificationManagerCompat.History.Clear();
                }
                catch
                {
                }

                this.Log($"Exit code : {e.ApplicationExitCode}");
            }

            base.OnExit(e);
        }

        /// <inheritdoc/>
        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            e.Cancel = true;
            base.OnSessionEnding(e);
            if (!singleInstanceChecker.IsExitDueToSingleInstanceRestriction)
            {
                Messenger.Send(new AppExitingMessage());
                switchableImplementationManager.UnInitialize();
                AutoWired<ISettingService>().UnInitialize();
                try
                {
                    ToastNotificationManagerCompat.History.Clear();
                }
                catch
                {
                }
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

        private void TriggerAppStartUpEvent()
        {
            pluginService.Plugins
                .OfType<IAppStartUp>()
                .ForEach(notified => notified.Happen(DI));
        }

        private void ConfigureUnhandledException()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!singleInstanceChecker.IsEnsureingSingleInstance)
            {
                // unhandled exception now can be uploaded automatically
                new ExceptionWindow((Exception)e.ExceptionObject).ShowDialog();
            }
        }

        private void ConfigureRequester()
        {
            Requester.ResponseFailedAction = (ex, method, desc) =>
            {
                if (ex is not TaskCanceledException)
                {
                    Crashes.TrackError(ex, new Dictionary<string, string> { { method, desc } });
                }
            };
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
            ThemeManager.Current.ApplicationTheme = Setting2.AppTheme.Get();

            // set app accent color to correct color.
            Accent.Change(ThemeManager.Current.ActualAccentColor, ThemeType.Unknown);
        }

        /// <summary>
        /// 默认容器实现
        /// </summary>
        internal class DefaultContainter : Core.IContainer
        {
            /// <summary>
            /// 在容器中查找注入的服务
            /// </summary>
            /// <typeparam name="T">注入的服务的类型</typeparam>
            /// <returns>注入的服务</returns>
            public T Find<T>()
                where T : class
            {
                return AutoWired<T>();
            }
        }
    }
}