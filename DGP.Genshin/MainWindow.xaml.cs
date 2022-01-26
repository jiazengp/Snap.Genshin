using DGP.Genshin.Control;
using DGP.Genshin.Control.Title;
using DGP.Genshin.Core.Plugins;
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
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
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
        //cause we do have some async operation in initialization so we can't use lock
        private readonly SemaphoreSlim initializingWindow = new(1, 1);
        private static bool hasInitializeCompleted = false;
        private static bool hasEverOpen = false;
        private readonly INavigationService navigationService;

        /// <summary>
        /// do not set datacontext for mainwindow
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            navigationService = App.GetService<INavigationService>();
            navigationService.NavigationView = NavView;
            navigationService.Frame = ContentFrame;

            App.Messenger.Register<MainWindow, SplashInitializationCompletedMessage>(this, (r, m) => r.Receive(m));
            App.Messenger.Register<MainWindow, NavigateRequestMessage>(this, (r, m) => r.Receive(m));
        }

        ~MainWindow()
        {
            App.Messenger.Unregister<SplashInitializationCompletedMessage>(this);
            App.Messenger.Unregister<NavigateRequestMessage>(this);
        }

        public async void Receive(SplashInitializationCompletedMessage viewModelReference)
        {
            initializingWindow.Wait();
            ISettingService settingService = App.GetService<ISettingService>();
            SplashViewModel splashViewModel = viewModelReference.Value;
            PrepareTitleBarArea();
            AddAditionalNavigationViewItem();
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
            initializingWindow.Release();
            hasInitializeCompleted = true;

            if (!hasEverOpen)
            {
                if (settingService.GetOrDefault(Setting.IsTaskBarIconEnabled, true))
                {
                    if (settingService.GetOrDefault(Setting.CloseMainWindowAfterInitializaion, false))
                    {
                        //before call Close() in this method,must release initializingWindow.
                        Close();
                    }
                }
            }

            hasEverOpen = true;
        }
        public void Receive(NavigateRequestMessage message)
        {
            navigationService.Navigate(message.Value, message.IsSyncTabRequested);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            initializingWindow.Wait();
            base.OnClosing(e);
            initializingWindow.Release();

            bool isTaskbarIconEnabled = App.GetService<ISettingService>().GetOrDefault(Setting.IsTaskBarIconEnabled, false);
            if (!hasInitializeCompleted || !isTaskbarIconEnabled)
            {
                App.Current.Shutdown();
            }
        }

        private static void DoTaskbarFlow()
        {
            App.Current.NotifyIcon ??= App.Current.FindResource("TaskbarIcon") as TaskbarIcon;
            if (App.Current.NotifyIcon is not null)
            {
                App.Current.NotifyIcon.DataContext = App.GetViewModel<TaskbarIconViewModel>();
            }
        }

        /// <summary>
        /// 添加从插件引入的额外的导航页面
        /// </summary>
        private void AddAditionalNavigationViewItem()
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

        /// <summary>
        /// 对Cookie列表内的所有角色签到
        /// </summary>
        /// <param name="splashView"></param>
        /// <returns></returns>
        private async Task SignInOnStartUp(SplashViewModel splashView)
        {
            DateTime? latsSignInTime = App.GetService<ISettingService>().GetOrDefault(
                Setting.LastAutoSignInTime, DateTime.Today.AddDays(-1), Setting.NullableDataTimeConverter);

            if (latsSignInTime < DateTime.Today)
            {
                splashView.CurrentStateDescription = "签到中...";
                await SignInAllAccountsRolesAsync();
            }
        }

        public static async Task SignInAllAccountsRolesAsync()
        {
            ICookieService cookieService = App.GetService<ICookieService>();
            ISettingService settingService = App.GetService<ISettingService>();

            cookieService.CookiesLock.EnterReadLock();
            foreach (string cookie in cookieService.Cookies)
            {
                List<UserGameRole> roles = await new UserGameRoleProvider(cookie).GetUserGameRolesAsync();
                foreach (UserGameRole role in roles)
                {
                    SignInResult? result = await new SignInProvider(cookie).SignInAsync(role);

                    settingService[Setting.LastAutoSignInTime] = DateTime.Now;
                    bool isSignInSilently = settingService.GetOrDefault(Setting.SignInSilently, false);
                    try
                    {
                        new ToastContentBuilder()
                        .AddSignInHeader("米游社每日签到")
                        .AddText(role.ToString())
                        .AddText(result is null ? "签到失败" : "签到成功")
                        .Show(toast => { toast.SuppressPopup = isSignInSilently; });
                    }
                    catch (DllNotFoundException) { }
                    catch (COMException) { }
                }
            }
            cookieService.CookiesLock.ExitReadLock();
        }

        #region Update
        private async void DoUpdateFlowAsync()
        {
            await CheckUpdateAsync();
            ISettingService settingService = App.GetService<ISettingService>();
            IUpdateService updateService = App.GetService<IUpdateService>();
            Version? lastLaunchAppVersion = settingService.GetOrDefault(Setting.AppVersion, updateService.CurrentVersion, Setting.VersionConverter);
            //first launch after update
            if (lastLaunchAppVersion < updateService.CurrentVersion)
            {
                settingService[Setting.AppVersion] = updateService.CurrentVersion;
                new WhatsNewWindow { ReleaseNote = updateService.Release?.Body }.Show();
            }
        }
        private async Task CheckUpdateAsync()
        {
            UpdateState result = await App.GetService<IUpdateService>().CheckUpdateStateAsync();
            //force-update, debug code
            //result = UpdateState.NeedUpdate;
            switch (result)
            {
                case UpdateState.NeedUpdate:
                    {
                        new ToastContentBuilder()
                            .AddText("有新的更新可用")
                            .AddText(App.GetService<IUpdateService>().NewVersion?.ToString())
                            .AddButton(new ToastButton().SetContent("更新").AddArgument("action", "update").SetBackgroundActivation())
                            .AddButton(new ToastButtonDismiss("忽略"))
                            .Show();
                        break;
                    }
                case UpdateState.NotAvailable:
                    {
                        new ToastContentBuilder()
                            .AddText("检查更新失败")
                            .Show();
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
