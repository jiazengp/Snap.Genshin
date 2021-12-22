using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Controls.Infrastructures.Markdown;
using DGP.Genshin.Controls.TitleBarButtons;
using DGP.Genshin.Core.Plugins;
using DGP.Genshin.Messages;
using DGP.Genshin.MiHoYoAPI.GameRole;
using DGP.Genshin.MiHoYoAPI.Sign;
using DGP.Genshin.Pages;
using DGP.Genshin.Services.Abstratcions;
using DGP.Genshin.Services.Notifications;
using DGP.Genshin.ViewModels;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace DGP.Genshin
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, IRecipient<SplashInitializationCompletedMessage>
    {
        private readonly INavigationService navigationService;
        public MainWindow()
        {
            InitializeComponent();
            navigationService = App.GetService<INavigationService>();
            navigationService.NavigationView = NavView;
            navigationService.Frame = ContentFrame;
            App.Messenger.Register<MainWindow, SplashInitializationCompletedMessage>(this, (r, m) => r.Receive(m));

            //do not set datacontext for mainwindow
            
            this.Log("initialized");
        }

        public async void Receive(SplashInitializationCompletedMessage message)
        {
            SplashViewModel splashView = message.Value;
            PrepareTitleBarArea(splashView);
            AddAditionalNavigationViewItem();
            DoUpdateFlow();
            //签到
            if (App.GetService<ISettingService>().GetOrDefault(Setting.AutoDailySignInOnLaunch, false))
            {
                await SignInOnStartUp(splashView);
            }
            splashView.CurrentStateDescription = "完成";
            splashView.IsSplashNotVisible = true;
            //post actions
            navigationService.Navigate<HomePage>(isSyncTabRequested: true);
        }

        /// <summary>
        /// 添加从插件引入的额外的导航页面
        /// </summary>
        private void AddAditionalNavigationViewItem()
        {
            foreach (var plugin in App.Current.ServiceManager.PluginService.Plugins)
            {
                foreach (var importPage in plugin.GetType().GetCustomAttributes<ImportPageAttribute>())
                {
                    navigationService.AddToNavigation(importPage);
                }
            }
        }

        /// <summary>
        /// 准备标题栏按钮
        /// </summary>
        /// <param name="splashView"></param>
        /// <returns></returns>
        private void PrepareTitleBarArea(SplashViewModel splashView)
        {
            UserInfoTitleBarButton UserInfoTitleButton = new();
            TitleBarStackPanel.Children.Add(UserInfoTitleButton);
            TitleBarStackPanel.Children.Add(new LaunchTitleBarButton());
            TitleBarStackPanel.Children.Add(new SignInTitleBarButton());
            TitleBarStackPanel.Children.Add(new JourneyLogTitleBarButton());
            TitleBarStackPanel.Children.Add(new DailyNoteTitleBarButton());
            splashView.CurrentStateDescription = "初始化用户信息...";
        }

        /// <summary>
        /// 对Cookie列表内的所有角色签到
        /// </summary>
        /// <param name="splashView"></param>
        /// <returns></returns>
        private static async Task SignInOnStartUp(SplashViewModel splashView)
        {
            DateTime? converter(object? str) => str is not null ? DateTime.Parse((string)str) : null;
            DateTime? latsSignInTime = App.GetService<ISettingService>().GetOrDefault(Setting.LastAutoSignInTime, DateTime.Today.AddDays(-1), converter);

            if (latsSignInTime < DateTime.Today)
            {
                splashView.CurrentStateDescription = "签到中...";
                foreach (string cookie in App.GetService<ICookieService>().Cookies)
                {
                    List<UserGameRole> roles = await new UserGameRoleProvider(cookie).GetUserGameRolesAsync();
                    foreach (UserGameRole role in roles)
                    {
                        SignInResult? result = await new SignInProvider(cookie).SignInAsync(role);
                        App.GetService<ISettingService>()[Setting.LastAutoSignInTime] = DateTime.Now;
                        new ToastContentBuilder()
                            .AddSignInHeader("米游社每日签到")
                            .AddText(role.ToString())
                            .AddText(result is null ? "签到失败" : "签到成功")
                            .Show(toast => { toast.SuppressPopup = App.GetService<ISettingService>().GetOrDefault(Setting.SignInSilently, false); });
                    }
                }
            }
        }

        #region Update
        private async void DoUpdateFlow()
        {
            await CheckUpdateAsync();
            ISettingService settingService = App.GetService<ISettingService>();
            IUpdateService? updateService = App.GetService<IUpdateService>();
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
            await new ContentDialog
            {
                Title = $"版本 {App.GetService<IUpdateService>().Release?.TagName} 更新日志",
                Content = new FlowDocumentScrollViewer
                {
                    Document = new TextToFlowDocumentConverter
                    {
                        Markdown = FindResource("Markdown") as Markdown
                    }.Convert(App.GetService<IUpdateService>().Release?.Body, typeof(FlowDocument), null, null) as FlowDocument
                },
                PrimaryButtonText = "了解",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
        }
        #endregion


    }
}
