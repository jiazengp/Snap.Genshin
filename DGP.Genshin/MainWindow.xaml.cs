using DGP.Genshin.Controls.Infrastructures.Markdown;
using DGP.Genshin.Controls.TitleBarButtons;
using DGP.Genshin.Core.Plugins;
using DGP.Genshin.Helpers.Notifications;
using DGP.Genshin.Messages;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Sign;
using DGP.Genshin.Pages;
using DGP.Genshin.Services.Abstratcions;
using DGP.Genshin.ViewModels;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace DGP.Genshin
{
    public partial class MainWindow : Window, IRecipient<SplashInitializationCompletedMessage>
    {
        //make sure while initializing exact components app main window can't be closed
        //prevent System.NullReferenceException
        private readonly SemaphoreSlim initializingWindow = new(1);
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
        }

        ~MainWindow()
        {
            App.Messenger.Unregister<SplashInitializationCompletedMessage>(this);
        }

        public async void Receive(SplashInitializationCompletedMessage viewModelReference)
        {
            initializingWindow.Wait();
            ISettingService settingService = App.GetService<ISettingService>();
            SplashViewModel splashView = viewModelReference.Value;
            PrepareTitleBarArea(splashView);
            AddAditionalNavigationViewItem();
            //preprocess
            if (!hasEverOpen)
            {
                splashView.CurrentStateDescription = "检查更新...";
                await DoUpdateFlowAsync();
                //签到
                if (App.GetService<ISettingService>().GetOrDefault(Setting.AutoDailySignInOnLaunch, false))
                {
                    await SignInOnStartUp(splashView);
                }
                if (settingService.GetOrDefault(Setting.IsTaskBarIconEnabled, true))
                {
                    //taskbar icon
                    App.Current.NotifyIcon ??= App.Current.FindResource("TaskbarIcon") as TaskbarIcon;
                    if (App.Current.NotifyIcon is not null)
                    {
                        App.Current.NotifyIcon.DataContext = App.GetViewModel<TaskbarIconViewModel>();
                    }
                }
                else
                {
                    App.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                }
            }
            splashView.CurrentStateDescription = "完成";
            splashView.IsSplashNotVisible = true;
            navigationService.Navigate<HomePage>(isSyncTabRequested: true);
            initializingWindow.Release();

            if (!hasEverOpen)
            {
                if (settingService.GetOrDefault(Setting.IsTaskBarIconEnabled, true))
                {
                    if (settingService.GetOrDefault(Setting.CloseMainWindowAfterInitializaion, false))
                    {
                        Close();
                    }
                }
            }

            hasEverOpen = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            initializingWindow.Wait();
            base.OnClosing(e);
            initializingWindow.Release();
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
        private void PrepareTitleBarArea(SplashViewModel splashView)
        {
            splashView.CurrentStateDescription = "初始化标题栏...";
            TitleBarStackPanel.Children.Add(new UserInfoTitleBarButton());
            TitleBarStackPanel.Children.Add(new LaunchTitleBarButton());
            TitleBarStackPanel.Children.Add(new SignInTitleBarButton());
            TitleBarStackPanel.Children.Add(new JourneyLogTitleBarButton());
            TitleBarStackPanel.Children.Add(new DailyNoteTitleBarButton());
        }

        /// <summary>
        /// 对Cookie列表内的所有角色签到
        /// </summary>
        /// <param name="splashView"></param>
        /// <returns></returns>
        private static async Task SignInOnStartUp(SplashViewModel splashView)
        {
            ISettingService settingService = App.GetService<ISettingService>();
            DateTime? latsSignInTime = settingService.GetOrDefault(
                Setting.LastAutoSignInTime, DateTime.Today.AddDays(-1), Setting.NullableDataTimeConverter);

            if (latsSignInTime < DateTime.Today)
            {
                splashView.CurrentStateDescription = "签到中...";

                ICookieService cookieService = App.GetService<ICookieService>();

                cookieService.CookiesLock.EnterReadLock();
                foreach (string cookie in cookieService.Cookies)
                {
                    List<UserGameRole> roles = await new UserGameRoleProvider(cookie).GetUserGameRolesAsync();
                    foreach (UserGameRole role in roles)
                    {
                        SignInResult? result = await new SignInProvider(cookie).SignInAsync(role);

                        settingService[Setting.LastAutoSignInTime] = DateTime.Now;
                        bool isSignInSilently = settingService.GetOrDefault(Setting.SignInSilently, false);
                        new ToastContentBuilder()
                            .AddSignInHeader("米游社每日签到")
                            .AddText(role.ToString())
                            .AddText(result is null ? "签到失败" : "签到成功")
                            .Show(toast => { toast.SuppressPopup = isSignInSilently; });
                    }
                }
                cookieService.CookiesLock.ExitReadLock();
            }
        }

        #region Update
        private async Task DoUpdateFlowAsync()
        {
            await CheckUpdateAsync();
            ISettingService settingService = App.GetService<ISettingService>();
            IUpdateService updateService = App.GetService<IUpdateService>();
            Version? lastLaunchAppVersion = settingService.GetOrDefault(Setting.AppVersion, updateService.CurrentVersion, Setting.VersionConverter);
            //first launch after update
            if (lastLaunchAppVersion < updateService.CurrentVersion)
            {
                settingService[Setting.AppVersion] = updateService.CurrentVersion;
                await ShowWhatsNewDialogAsync();
            }
        }
        private async Task CheckUpdateAsync()
        {
            UpdateState result = await App.GetService<IUpdateService>().CheckUpdateStateAsync();
            //update debug code here
            //if (Debugger.IsAttached) result = UpdateState.NeedUpdate;
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
        private async Task ShowWhatsNewDialogAsync()
        {
            IUpdateService updateService = App.GetService<IUpdateService>();
            await new ContentDialog
            {
                Title = $"{updateService.Release?.TagName} 更新日志",
                Content = new FlowDocumentScrollViewer
                {
                    Document = new TextToFlowDocumentConverter
                    {
                        Markdown = FindResource("Markdown") as Markdown
                    }.Convert(updateService.Release?.Body, typeof(FlowDocument), null, null) as FlowDocument
                },
                PrimaryButtonText = "了解",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
        }
        #endregion
    }
}
