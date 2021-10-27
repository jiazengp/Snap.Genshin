using DGP.Genshin.Controls;
using DGP.Genshin.Controls.Infrastructures.Markdown;
using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.Sign;
using DGP.Genshin.MiHoYoAPI.User;
using DGP.Genshin.Pages;
using DGP.Genshin.Services;
using DGP.Genshin.Services.Settings;
using DGP.Genshin.Services.Updating;
using DGP.Genshin.Common.Extensions.System;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf.Controls;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Collections.Generic;

namespace DGP.Genshin
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NavigationService navigationService;

        public MainWindow()
        {
            //do not set datacontext for mainwindow
            InitializeComponent();
            MainSplashView.InitializationPostAction += SplashInitializeCompleted;

            navigationService = new NavigationService(this, NavView, ContentFrame);

            this.Log("initialized");
        }
        /// <summary>
        /// 前初始化工作已经完成
        /// </summary>
        /// <param name="splashView"></param>
        private async void SplashInitializeCompleted(SplashView splashView)
        {
            splashView.CurrentStateDescription = "检查程序更新...";
            await CheckUpdateAsync();

            splashView.CurrentStateDescription = "初始化用户信息...";
            await UserInfoTitleButton.RefreshUserInfosAsync();

            //签到
            if (SettingService.Instance.GetOrDefault(Setting.AutoDailySignInOnLaunch, false))
            {
                await SignInOnStartUp(splashView);
            }

            splashView.CurrentStateDescription = "完成";
            //post actions
            if (!navigationService.HasEverNavigated)
            {
                navigationService.Navigate<HomePage>(isSyncTabRequested: true);
            }
            splashView.HasCheckCompleted = true;
        }

        private static async Task SignInOnStartUp(SplashView splashView)
        {
            DateTime? converter(object? str) => str is not null ? DateTime.Parse((string)str) : null;
            DateTime? latsSignInTime = SettingService.Instance.GetOrDefault(Setting.LastAutoSignInTime, DateTime.Today.AddDays(-1), converter);

            if (latsSignInTime < DateTime.Today)
            {
                splashView.CurrentStateDescription = "签到中...";
                UserGameRoleInfo? roleInfo = await new UserGameRoleProvider(CookieManager.CurrentCookie).GetUserGameRolesAsync();
                List<UserGameRole>? list = roleInfo?.List;
                if (list is not null)
                {
                    foreach (UserGameRole role in list)
                    {
                        SignInResult? result = await new SignInProvider(CookieManager.CurrentCookie).SignInAsync(role);
                        SettingService.Instance[Setting.LastAutoSignInTime] = DateTime.Now;
                        new ToastContentBuilder()
                            .AddText(result is null ? "签到失败" : "签到成功")
                            .AddText(role.ToString())
                            .AddAttributionText("米游社每日签到")
                            .Show();
                    }
                }
            }
        }

        #region Update
        //TODO:make update to display in toast
        private async Task CheckUpdateAsync()
        {
            UpdateState result = await UpdateService.Instance.CheckUpdateStateAsync();

            string hint = result switch
            {
                UpdateState.NeedUpdate => "检测到更新",
                UpdateState.IsNewestRelease => "",
                UpdateState.IsInsiderVersion => "",
                UpdateState.NotAvailable => "",
                _ => throw new NotImplementedException(),
            };

            switch (result)
            {
                case UpdateState.NeedUpdate:
                    {
                        if (await ShowConfirmUpdateDialogAsync() == ContentDialogResult.Primary)
                        {
                            new ToastContentBuilder()
                            .AddText("有新的更新可用")
                            .AddText(UpdateService.Instance.NewVersion?.ToString())
                            .AddButton(new ToastButton().SetContent("更新").AddArgument("action", "update").SetBackgroundActivation())
                            .AddButton(new ToastButtonDismiss("忽略"))
                            .Show();
                            UpdateService.Instance.DownloadAndInstallPackage();
                            await new UpdateDialog().ShowAsync();
                        }
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

        private async Task<ContentDialogResult> ShowConfirmUpdateDialogAsync()
        {
            Markdown? m = FindResource("Markdown") as Markdown;
            Debug.Assert(m is not null);
            return await new ContentDialog
            {
                Title = UpdateService.Instance.Release?.Name,
                Content = new FlowDocumentScrollViewer
                {
                    Document = new TextToFlowDocumentConverter
                    {
                        Markdown = m
                    }.Convert(UpdateService.Instance.Release?.Body, typeof(FlowDocument), null, null) as FlowDocument
                },
                PrimaryButtonText = "更新",
                CloseButtonText = "忽略",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
        }
        #endregion
    }
}
