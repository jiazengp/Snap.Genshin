using DGP.Genshin.Control;
using DGP.Genshin.Core.Notification;
using DGP.Genshin.Core.PerMonitorDPIAware;
using DGP.Genshin.Core.Plugins;
using DGP.Genshin.DataModel.WebViewLobby;
using DGP.Genshin.Helper;
using DGP.Genshin.Message;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstraction;
using DGP.Genshin.ViewModel;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.VisualStudio.Threading;
using Snap.Reflection;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DGP.Genshin
{
    public partial class MainWindow : Window,
        IRecipient<SplashInitializationCompletedMessage>,
        IRecipient<NavigateRequestMessage>,
        IRecipient<BackgroundOpacityChangedMessage>
    {
        //make sure while post-initializing, main window can't be closed
        //prevent System.NullReferenceException
        //cause we have some async operation in initialization so we can't use lock
        private readonly SemaphoreSlim initializingWindow = new(1, 1);
        private bool hasInitializeCompleted = false;

        private static bool hasEverOpen = false;
        private static bool hasEverClose = false;

        private readonly INavigationService navigationService;
        private readonly BackgroundLoader backgroundLoader;

        /// <summary>
        /// do NOT set DataContext for mainwindow
        /// </summary>
        public MainWindow()
        {
            InitializeContent();
            _ = new PerMonitorDPIAdapter(this);
            //randomly load a image as background
            backgroundLoader = new(this);
            backgroundLoader.LoadWallpaper();
            //initialize NavigationService
            navigationService = App.AutoWired<INavigationService>();
            navigationService.NavigationView = NavView;
            navigationService.Frame = ContentFrame;
            //register messages
            App.Messenger.Register<SplashInitializationCompletedMessage>(this);
            App.Messenger.Register<NavigateRequestMessage>(this);
            App.Messenger.Register<BackgroundOpacityChangedMessage>(this);
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
            App.Messenger.Unregister<SplashInitializationCompletedMessage>(this);
            App.Messenger.Unregister<NavigateRequestMessage>(this);
            App.Messenger.Unregister<BackgroundOpacityChangedMessage>(this);
        }

        public void Receive(SplashInitializationCompletedMessage viewModelReference)
        {
            PostInitializeAsync(viewModelReference).Forget();
        }

        private async Task PostInitializeAsync(SplashInitializationCompletedMessage viewModelReference)
        {
            await initializingWindow.WaitAsync();
            SplashViewModel splashViewModel = viewModelReference.Value;
            AddAdditionalWebViewNavigationViewItems();
            AddAdditionalPluginsNavigationViewItems();
            //preprocess
            if (!hasEverOpen)
            {
                DoUpdateFlowAsync().Forget();
                //签到
                if (Setting2.AutoDailySignInOnLaunch.Get())
                {
                    await SignInOnStartUpAsync(splashViewModel);
                }
                //任务栏
                if (Setting2.IsTaskBarIconEnabled.Get())
                {
                    DoTaskbarFlow();
                }
                //树脂服务
                App.AutoWired<IDailyNoteService>().Initialize();
            }
            splashViewModel.CurrentStateDescription = "完成";
            splashViewModel.IsSplashNotVisible = true;
            await Task.Delay(800);
            navigationService.Navigate<HomePage>(isSyncTabRequested: true);
            //before call Close() in this method,must release initializingWindow.
            initializingWindow.Release();
            hasInitializeCompleted = true;

            if (!hasEverOpen)
            {
                if (Setting2.IsTaskBarIconEnabled.Get() && (App.Current.NotifyIcon is not null))
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

        public void Receive(NavigateRequestMessage message)
        {
            navigationService.Navigate(message);
        }
        public void Receive(BackgroundOpacityChangedMessage message)
        {
            if (BackgroundGrid.Background is ImageBrush brush)
            {
                brush.Opacity = message.Value;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                Setting2.MainWindowWidth.Set(Width);
                Setting2.MainWindowHeight.Set(Height);
            }
            Setting2.IsNavigationViewPaneOpen.Set(NavView.IsPaneOpen);
            initializingWindow.Wait();
            base.OnClosing(e);
            initializingWindow.Release();

            bool isTaskbarIconEnabled = Setting2.IsTaskBarIconEnabled.Get() && (App.Current.NotifyIcon is not null);

            if (hasInitializeCompleted && isTaskbarIconEnabled)
            {
                if (Setting2.IsTaskBarIconHintDisplay.Get() && (!hasEverClose))
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

        private void DoTaskbarFlow()
        {
            App.Current.NotifyIcon ??= App.Current.FindResource("TaskbarIcon") as TaskbarIcon;
            App.Current.NotifyIcon!.DataContext = App.AutoWired<TaskbarIconViewModel>();
        }

        #region Aditional NavigationViewItems
        /// <summary>
        /// 添加从插件引入的额外的导航页签
        /// </summary>
        private void AddAdditionalPluginsNavigationViewItems()
        {
            foreach (IPlugin plugin in App.Current.PluginService.Plugins)
            {
                plugin.ForEachAttribute<ImportPageAttribute>(importPage => navigationService.AddToNavigation(importPage));
            }
        }
        /// <summary>
        /// 添加额外的网页导航页签
        /// </summary>
        private void AddAdditionalWebViewNavigationViewItems()
        {
            ObservableCollection<WebViewEntry>? entries = App.AutoWired<WebViewLobbyViewModel>().Entries;
            navigationService.AddWebViewEntries(entries);
        }
        #endregion

        #region Sign In
        /// <summary>
        /// 对Cookie列表内的所有角色签到
        /// </summary>
        /// <param name="splashView"></param>
        /// <returns></returns>
        private async Task SignInOnStartUpAsync(SplashViewModel splashView)
        {
            if (Setting2.LastAutoSignInTime.Get() < DateTime.Today)
            {
                splashView.CurrentStateDescription = "签到中...";
                await App.AutoWired<ISignInService>().TrySignAllAccountsRolesInAsync();
            }
        }
        #endregion

        #region Update
        private async Task DoUpdateFlowAsync()
        {
            await CheckUpdateAsync();
            IUpdateService updateService = App.AutoWired<IUpdateService>();
            Version? lastLaunchAppVersion = Setting2.AppVersion.Get();
            //first launch after update
            if (lastLaunchAppVersion < updateService.CurrentVersion)
            {
                Setting2.AppVersion.Set(updateService.CurrentVersion);
                //App.Current.Dispatcher.InvokeAsync
                new WhatsNewWindow { ReleaseNote = updateService.Release?.Body }.Show();
            }
        }
        private async Task CheckUpdateAsync()
        {
            UpdateState result = await App.AutoWired<IUpdateService>().CheckUpdateStateAsync();
            //force-update, debug code
            //result = UpdateState.NeedUpdate;
            switch (result)
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
    }
}
