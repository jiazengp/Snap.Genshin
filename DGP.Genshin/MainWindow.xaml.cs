using CommunityToolkit.Mvvm.Messaging;
using DGP.Genshin.Core.Background;
using DGP.Genshin.Core.DpiAware;
using DGP.Genshin.Core.Notification;
using DGP.Genshin.Core.Plugins;
using DGP.Genshin.Message.Internal;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstraction;
using DGP.Genshin.Service.Abstraction.Setting;
using DGP.Genshin.Service.Abstraction.Updating;
using DGP.Genshin.ViewModel;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.VisualStudio.Threading;
using Snap.Extenion.Enumerable;
using Snap.Reflection;
using Snap.Threading;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Genshin
{
    /// <summary>
    /// 主窗体
    /// </summary>
    internal partial class MainWindow : Window, IRecipient<SplashInitializationCompletedMessage>
    {
        private static bool hasEverOpen = false;
        private static bool hasEverClose = false;

        // make sure while post-initializing, main window can't be closed
        // prevent System.NullReferenceException
        // cause we have some async operation in initialization so we can't use lock
        private readonly SemaphoreSlim initializingWindow = new(1, 1);
        private readonly INavigationService navigationService;
        private readonly BackgroundLoader backgroundLoader;

        private bool hasInitializationCompleted = false;

        /// <summary>
        /// 构造新的主窗体的实例
        /// Do NOT set DataContext for mainwindow
        /// </summary>
        public MainWindow()
        {
            this.InitializeContent();

            // support per monitor dpi awareness
            _ = new DpiAwareAdapter(this);

            // randomly load a image as background
            this.backgroundLoader = new(this, App.Messenger);
            this.backgroundLoader.LoadNextWallpaperAsync().Forget();

            // initialize NavigationService
            this.navigationService = App.AutoWired<INavigationService>();
            this.navigationService.NavigationView = this.NavView;
            this.navigationService.Frame = this.ContentFrame;

            // register messages
            App.Messenger.RegisterAll(this);
        }

        /// <summary>
        /// 释放消息器资源
        /// </summary>
        ~MainWindow()
        {
            App.Messenger.UnregisterAll(this);
        }

        /// <inheritdoc/>
        public void Receive(SplashInitializationCompletedMessage viewModelReference)
        {
            this.PostInitializeAsync(viewModelReference).Forget();
        }

        /// <inheritdoc/>
        protected override void OnClosing(CancelEventArgs e)
        {
            this.TrySaveWindowState();
            if (this.initializingWindow.CurrentCount < 1)
            {
                e.Cancel = true;
                return;
            }

            using (this.initializingWindow.Enter())
            {
                base.OnClosing(e);
            }

            bool isTaskbarIconEnabled = Setting2.IsTaskBarIconEnabled.Get() && (App.Current.NotifyIcon is not null);

            if (this.hasInitializationCompleted && isTaskbarIconEnabled)
            {
                if ((!hasEverClose) && Setting2.IsTaskBarIconHintDisplay.Get())
                {
                    new ToastContentBuilder()
                    .AddText("Snap Genshin 已转入后台运行")
                    .AddText("点击托盘图标以显示主窗口")
                    .AddButton(new ToastButton()
                        .SetContent("不再显示")
                        .AddArgument("taskbarhint", "hide")
                        .SetBackgroundActivation())
                    .SafeShow(false);
                    hasEverClose = true;
                }
            }
            else
            {
                App.Current.Shutdown();
            }
        }

        private void InitializeContent()
        {
            this.InitializeComponent();

            // restore width and height from setting
            this.Width = Setting2.MainWindowWidth.Get();
            this.Height = Setting2.MainWindowHeight.Get();

            // restore pane state
            this.NavView.IsPaneOpen = Setting2.IsNavigationViewPaneOpen.Get();
        }

        private async Task PostInitializeAsync(SplashInitializationCompletedMessage viewModelReference)
        {
            using (await this.initializingWindow.EnterAsync())
            {
                SplashViewModel splashViewModel = viewModelReference.Value;
                this.AddAdditionalNavigationViewItems();

                // preprocess
                if (!hasEverOpen)
                {
                    this.CheckUpdateForWhatsNewAsync().Forget();
                    this.TrySignInOnStartUpAsync().Forget();

                    this.TryInitializeTaskbarIcon();

                    // 树脂服务
                    App.AutoWired<IDailyNoteService>().Initialize();
                }

                splashViewModel.CompleteInitialization();

                await Task.Delay(TimeSpan.FromMilliseconds(800));
                this.navigationService.Navigate<HomePage>(isSyncTabRequested: true);
            }

            this.hasInitializationCompleted = true;

            if (!hasEverOpen)
            {
                if (Setting2.IsTaskBarIconEnabled && (App.Current.NotifyIcon is not null))
                {
                    if ((!App.IsLaunchedByUser) && Setting2.CloseMainWindowAfterInitializaion.Get())
                    {
                        this.Close();
                    }
                }
            }

            // 设置已经打开过状态
            hasEverOpen = true;
        }

        private void AddAdditionalNavigationViewItems()
        {
            // webview entries must add first
            this.navigationService.AddWebViewEntries(App.AutoWired<WebViewLobbyViewModel>().Entries);

            // then we add pilugin pages
            App.Current.PluginService.Plugins.ForEach(plugin =>
            plugin.ForEachAttribute<ImportPageAttribute>(importPage =>
            this.navigationService.AddToNavigation(importPage)));
        }

        private void TrySaveWindowState()
        {
            if (this.WindowState == WindowState.Normal)
            {
                Setting2.MainWindowWidth.Set(this.Width);
                Setting2.MainWindowHeight.Set(this.Height);
            }

            Setting2.IsNavigationViewPaneOpen.Set(this.NavView.IsPaneOpen);
        }

        private void TryInitializeTaskbarIcon()
        {
            if (Setting2.IsTaskBarIconEnabled.Get() && App.Current.NotifyIcon is null)
            {
                App.Current.NotifyIcon = App.Current.FindResource("TaskbarIcon") as TaskbarIcon;
                App.Current.NotifyIcon!.DataContext = App.AutoWired<TaskbarIconViewModel>();
            }
        }

        private async Task TrySignInOnStartUpAsync()
        {
            if (Setting2.AutoDailySignInOnLaunch.Get())
            {
                if (Setting2.LastAutoSignInTime.Get() < DateTime.Today)
                {
                    await App.AutoWired<ISignInService>().TrySignAllAccountsRolesInAsync();
                }
            }
        }

        private async Task CheckUpdateForWhatsNewAsync()
        {
            await this.CheckUpdateForNotificationAsync();
            IUpdateService updateService = App.AutoWired<IUpdateService>();
            Setting2.AppVersion.Set(updateService.CurrentVersion);
        }

        private async Task CheckUpdateForNotificationAsync()
        {
            switch (await App.AutoWired<IUpdateService>().CheckUpdateStateAsync())
            {
                case UpdateState.NeedUpdate:
                    {
                        new ToastContentBuilder()
                            .AddText("有新的更新可用")
                            .AddText(App.AutoWired<IUpdateService>().NewVersion?.ToString())
                            .AddButton(new ToastButton()
                                .SetContent("更新")
                                .AddArgument("action", "update")
                                .SetBackgroundActivation())
                            .AddButton(new ToastButtonDismiss("忽略"))
                            .SafeShow();
                        break;
                    }

                case UpdateState.NotAvailable:
                    {
                        new ToastContentBuilder()
                            .AddText("检查更新失败")
                            .AddText("无法连接到 Github")
                            .SafeShow();
                        break;
                    }

                case UpdateState.IsNewestRelease:
                case UpdateState.IsInsiderVersion:
                default:
                    break;
            }
        }
    }
}