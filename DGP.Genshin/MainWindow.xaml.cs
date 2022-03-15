using DGP.Genshin.Core.Background;
using DGP.Genshin.Core.DpiAware;
using DGP.Genshin.Core.Notification;
using DGP.Genshin.Core.Plugins;
using DGP.Genshin.Message;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstraction;
using DGP.Genshin.Service.Abstraction.Setting;
using DGP.Genshin.Service.Abstraction.Updating;
using DGP.Genshin.ViewModel;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.VisualStudio.Threading;
using Snap.Extenion.Enumerable;
using Snap.Reflection;
using Snap.Threading;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Genshin
{
    /// <summary>
    /// 主窗体
    /// </summary>
    public partial class MainWindow : Window, IRecipient<SplashInitializationCompletedMessage>
    {
        //make sure while post-initializing, main window can't be closed
        //prevent System.NullReferenceException
        //cause we have some async operation in initialization so we can't use lock
        private readonly SemaphoreSlim initializingWindow = new(1, 1);

        private bool hasInitializationCompleted = false;

        private static bool hasEverOpen = false;
        private static bool hasEverClose = false;

        private readonly INavigationService navigationService;
        private readonly BackgroundLoader backgroundLoader;

        /// <summary>
        /// 构造新的主窗体的实例
        /// Do NOT set DataContext for mainwindow
        /// </summary>
        public MainWindow()
        {
            InitializeContent();
            //support per monitor dpi awareness
            _ = new DpiAwareAdapter(this);
            //randomly load a image as background
            backgroundLoader = new(this, App.Messenger);
            backgroundLoader.LoadNextWallpaperAsync().Forget();
            //initialize NavigationService
            navigationService = App.AutoWired<INavigationService>();
            navigationService.NavigationView = NavView;
            navigationService.Frame = ContentFrame;
            //register messages
            App.Messenger.RegisterAll(this);
        }

        private void InitializeContent()
        {
            InitializeComponent();
            //restore width and height from setting
            Width = Setting2.MainWindowWidth.Get();
            Height = Setting2.MainWindowHeight.Get();
            //restore pane state
            NavView.IsPaneOpen = Setting2.IsNavigationViewPaneOpen.Get();
        }

        ~MainWindow()
        {
            App.Messenger.UnregisterAll(this);
        }

        public void Receive(SplashInitializationCompletedMessage viewModelReference)
        {
            PostInitializeAsync(viewModelReference).Forget();
        }
        private async Task PostInitializeAsync(SplashInitializationCompletedMessage viewModelReference)
        {
            using (await initializingWindow.EnterAsync())
            {
                SplashViewModel splashViewModel = viewModelReference.Value;
                AddAdditionalNavigationViewItems();
                //preprocess
                if (!hasEverOpen)
                {
                    CheckUpdateForWhatsNewAsync().Forget();
                    TrySignInOnStartUpAsync().Forget();
                    TryInitializeTaskbarIcon();
                    //树脂服务
                    App.AutoWired<IDailyNoteService>().Initialize();
                }
                splashViewModel.CompleteInitialization();

                await Task.Delay(800);
                navigationService.Navigate<HomePage>(isSyncTabRequested: true);
            }
            hasInitializationCompleted = true;

            if (!hasEverOpen)
            {
                if (Setting2.IsTaskBarIconEnabled && (App.Current.NotifyIcon is not null))
                {
                    if ((!App.IsLaunchedByUser) && Setting2.CloseMainWindowAfterInitializaion.Get())
                    {
                        Close();
                    }
                }
            }
            //设置已经打开过状态
            hasEverOpen = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            TrySaveWindowState();
            if (initializingWindow.CurrentCount < 1)
            {
                e.Cancel = true;
                return;
            }
            using (initializingWindow.Enter())
            {
                base.OnClosing(e);
            }

            bool isTaskbarIconEnabled = Setting2.IsTaskBarIconEnabled.Get() && (App.Current.NotifyIcon is not null);

            if (hasInitializationCompleted && isTaskbarIconEnabled)
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

        /// <summary>
        /// 添加额外的导航页签
        /// </summary>
        private void AddAdditionalNavigationViewItems()
        {
            //webview entries must add first
            navigationService.AddWebViewEntries(App.AutoWired<WebViewLobbyViewModel>().Entries);
            //then we add pilugin pages
            App.Current.PluginService.Plugins.ForEach(plugin =>
            plugin.ForEachAttribute<ImportPageAttribute>(importPage =>
            navigationService.AddToNavigation(importPage)));
        }
        private void TrySaveWindowState()
        {
            if (WindowState == WindowState.Normal)
            {
                Setting2.MainWindowWidth.Set(Width);
                Setting2.MainWindowHeight.Set(Height);
            }
            Setting2.IsNavigationViewPaneOpen.Set(NavView.IsPaneOpen);
        }
        private void TryInitializeTaskbarIcon()
        {
            if (Setting2.IsTaskBarIconEnabled.Get() && App.Current.NotifyIcon is null)
            {
                App.Current.NotifyIcon = App.Current.FindResource("TaskbarIcon") as TaskbarIcon;
                App.Current.NotifyIcon!.DataContext = App.AutoWired<TaskbarIconViewModel>();
            }
        }
        /// <summary>
        /// 对Cookie列表内的所有角色签到
        /// </summary>
        /// <param name="splashView"></param>
        /// <returns></returns>
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

        #region Update
        private async Task CheckUpdateForWhatsNewAsync()
        {
            await CheckUpdateForNotificationAsync();
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
        #endregion

        private void SwitchWallPaperButtonClick(object sender, RoutedEventArgs e)
        {
            backgroundLoader.LoadNextWallpaperAsync().Forget();
        }
    }
}
