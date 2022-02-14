using DGP.Genshin.Control;
using DGP.Genshin.Control.Title;
using DGP.Genshin.Core.Plugins;
using DGP.Genshin.DataModel.WebViewLobby;
using DGP.Genshin.Helper.Notification;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Sign;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstratcion;
using DGP.Genshin.ViewModel;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DGP.Genshin
{
    public partial class MainWindow : Window,
        IRecipient<SplashInitializationCompletedMessage>,
        IRecipient<NavigateRequestMessage>
    {
        //make sure while post-initializing, main window can't be closed
        //prevent System.NullReferenceException
        //cause we have some async operation in initialization so we can't use lock
        private readonly SemaphoreSlim initializingWindow = new(1, 1);
        private bool hasInitializeCompleted = false;
        private static bool hasEverOpen = false;
        private static bool hasEverClose = false;
        private readonly INavigationService navigationService;

        /// <summary>
        /// do not set DataContext for mainwindow
        /// </summary>
        public MainWindow()
        {
            InitializeContent();
            //initialize NavigationService
            navigationService = App.AutoWired<INavigationService>();
            navigationService.NavigationView = NavView;
            navigationService.Frame = ContentFrame;
            //register messages
            App.Messenger.Register<SplashInitializationCompletedMessage>(this);
            App.Messenger.Register<NavigateRequestMessage>(this);
        }

        private void InitializeContent()
        {
            InitializeComponent();
            //restore width and height from setting
            ISettingService settingService = App.AutoWired<ISettingService>();
            Width = settingService.GetOrDefault(Setting.MainWindowWidth, 0D);
            Height = settingService.GetOrDefault(Setting.MainWindowHeight, 0D);
            //restore pane state
            NavView.IsPaneOpen = settingService.GetOrDefault(Setting.IsNavigationViewPaneOpen, true);
        }

        ~MainWindow()
        {
            App.Messenger.Unregister<SplashInitializationCompletedMessage>(this);
            App.Messenger.Unregister<NavigateRequestMessage>(this);
        }

        public async void Receive(SplashInitializationCompletedMessage viewModelReference)
        {
            initializingWindow.Wait();
            ISettingService settingService = App.AutoWired<ISettingService>();
            SplashViewModel splashViewModel = viewModelReference.Value;
            PrepareTitleBarArea();
            AddAditionalWebViewNavigationViewItems();
            AddAditionalPluginsNavigationViewItems();
            //preprocess
            if (!hasEverOpen)
            {
                DoUpdateFlowAsync();
                //签到
                if (settingService.GetOrDefault(Setting.AutoDailySignInOnLaunch, false))
                {
                    await SignInOnStartUp(splashViewModel);
                }
                //任务栏
                if (settingService.GetOrDefault(Setting.IsTaskBarIconEnabled, true))
                {
                    DoTaskbarFlow();
                }
            }
            splashViewModel.CurrentStateDescription = "完成";
            splashViewModel.IsSplashNotVisible = true;
            navigationService.Navigate<HomePage>(isSyncTabRequested: true);
            //before call Close() in this method,must release initializingWindow.
            initializingWindow.Release();
            hasInitializeCompleted = true;

            if (!hasEverOpen)
            {
                if (settingService.GetOrDefault(Setting.IsTaskBarIconEnabled, true) && (App.Current.NotifyIcon is not null))
                {
                    if (settingService.GetOrDefault(Setting.CloseMainWindowAfterInitializaion, false))
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
            navigationService.Navigate(message.Value, message.IsSyncTabRequested, message.ExtraData);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ISettingService settingService = App.AutoWired<ISettingService>();
            settingService[Setting.IsNavigationViewPaneOpen] = NavView.IsPaneOpen;
            initializingWindow.Wait();
            base.OnClosing(e);
            initializingWindow.Release();

            bool isTaskbarIconEnabled = settingService.GetOrDefault(Setting.IsTaskBarIconEnabled, false)
                && (App.Current.NotifyIcon is not null);

            if (hasInitializeCompleted && isTaskbarIconEnabled)
            {
                if (!hasEverClose)
                {
                    SecureToastNotificationContext.TryCatch(() =>
                    new ToastContentBuilder()
                    .AddText("Snap Genshin 已转入后台运行\n点击托盘图标以显示主窗口")
                    .Show());
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

        /// <summary>
        /// 添加从插件引入的额外的导航页签
        /// </summary>
        private void AddAditionalPluginsNavigationViewItems()
        {
            foreach (IPlugin? plugin in App.Current.PluginService.Plugins)
            {
                foreach (ImportPageAttribute? importPage in plugin.GetType().GetCustomAttributes<ImportPageAttribute>())
                {
                    navigationService.AddToNavigation(importPage);
                }
            }
        }
        /// <summary>
        /// 添加额外的网页导航页签
        /// </summary>
        private void AddAditionalWebViewNavigationViewItems()
        {
            ObservableCollection<WebViewEntry>? entries = App.AutoWired<WebViewLobbyViewModel>().Entries;
            navigationService.AddWebViewEntries(entries);
        }

        /// <summary>
        /// 准备标题栏按钮
        /// </summary>
        /// <param name="splashView"></param>
        private void PrepareTitleBarArea()
        {
            TitleBarStackPanel.Children.Add(new LaunchTitleBarButton());
            TitleBarStackPanel.Children.Add(new DailyNoteTitleBarButton());
            TitleBarStackPanel.Children.Add(new SignInTitleBarButton());
            TitleBarStackPanel.Children.Add(new JourneyLogTitleBarButton());
            TitleBarStackPanel.Children.Add(new UserInfoTitleBarButton());
        }

        #region Sign In
        /// <summary>
        /// 对Cookie列表内的所有角色签到
        /// </summary>
        /// <param name="splashView"></param>
        /// <returns></returns>
        private async Task SignInOnStartUp(SplashViewModel splashView)
        {
            DateTime? latsSignInTime = App.AutoWired<ISettingService>().GetOrDefault(
                Setting.LastAutoSignInTime, DateTime.Today.AddDays(-1), Setting.NullableDataTimeConverter);

            if (latsSignInTime < DateTime.Today)
            {
                splashView.CurrentStateDescription = "签到中...";
                await SignInAllAccountsRolesAsync();
            }
        }

        public static async Task SignInAllAccountsRolesAsync()
        {
            ICookieService cookieService = App.AutoWired<ICookieService>();
            ISettingService settingService = App.AutoWired<ISettingService>();

            cookieService.CookiesLock.EnterReadLock();
            foreach (string cookie in cookieService.Cookies)
            {
                List<UserGameRole> roles = await new UserGameRoleProvider(cookie).GetUserGameRolesAsync();
                foreach (UserGameRole role in roles)
                {
                    SignInResult? result = await new SignInProvider(cookie).SignInAsync(role);

                    settingService[Setting.LastAutoSignInTime] = DateTime.Now;
                    bool isSignInSilently = settingService.GetOrDefault(Setting.SignInSilently, false);
                    SecureToastNotificationContext.TryCatch(() =>
                    new ToastContentBuilder()
                        .AddSignInHeader("米游社每日签到")
                        .AddText(role.ToString())
                        .AddText(result is null ? "签到失败" : "签到成功")
                        .Show(toast => { toast.SuppressPopup = isSignInSilently; }));
                }
            }
            cookieService.CookiesLock.ExitReadLock();
        }
        #endregion

        #region Update
        private async void DoUpdateFlowAsync()
        {
            await CheckUpdateAsync();
            ISettingService settingService = App.AutoWired<ISettingService>();
            IUpdateService updateService = App.AutoWired<IUpdateService>();
            Version? lastLaunchAppVersion = settingService.GetOrDefault(Setting.AppVersion, updateService.CurrentVersion, Setting.VersionConverter);
            //first launch after update
            if (lastLaunchAppVersion < updateService.CurrentVersion)
            {
                settingService[Setting.AppVersion] = updateService.CurrentVersion;
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
                        SecureToastNotificationContext.TryCatch(() =>
                        new ToastContentBuilder()
                            .AddText("有新的更新可用")
                            .AddText(App.AutoWired<IUpdateService>().NewVersion?.ToString())
                            .AddButton(new ToastButton()
                                .SetContent("更新")
                                .AddArgument("action", "update")
                                .SetBackgroundActivation())
                            .AddButton(new ToastButtonDismiss("忽略"))
                            .Show());
                        break;
                    }
                case UpdateState.NotAvailable:
                    {
                        SecureToastNotificationContext.TryCatch(() =>
                        new ToastContentBuilder()
                            .AddText("检查更新失败")
                            .Show());
                        break;
                    }
                case UpdateState.IsNewestRelease:
                case UpdateState.IsInsiderVersion:
                default:
                    break;
            }
        }
        #endregion

        private void MainWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ISettingService settingService = App.AutoWired<ISettingService>();
            if (WindowState == WindowState.Normal)
            {
                settingService[Setting.MainWindowWidth] = Width;
                settingService[Setting.MainWindowHeight] = Height;
            }
        }
    }
}
