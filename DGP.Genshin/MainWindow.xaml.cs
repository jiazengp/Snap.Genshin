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

        private async void SplashInitializeCompleted(SplashView splashView)
        {
            splashView.CurrentStateDescription = "检查程序更新...";
            await CheckUpdateAsync();

            splashView.CurrentStateDescription = "初始化用户信息...";
            await UserInfoTitleButton.RefreshAsync();

            //签到
            if (SettingService.Instance.GetOrDefault(Setting.AutoDailySignInOnLaunch, false))
            {
                DateTime? time = SettingService.Instance.GetOrDefault(
                    Setting.LastAutoSignInTime,
                    DateTime.Today.AddDays(-1),
                    str => str is not null ? DateTime.Parse((string)str) : (DateTime?)null);

                if (time <= DateTime.Today)
                {
                    splashView.CurrentStateDescription = "签到中...";
                    UserGameRoleInfo? roleInfo = new UserGameRoleProvider(CookieManager.Cookie).GetUserGameRoles();
                    System.Collections.Generic.List<UserGameRole>? list = roleInfo?.List;
                    if (list is not null)
                    {
                        foreach (UserGameRole role in list)
                        {
                            SignInResult? result = await Task.Run(() => new SignInProvider(CookieManager.Cookie).SignIn(role));
                            new ToastContentBuilder()
                                .AddText(result is null ? "签到失败" : "签到成功")
                                .AddText(role.ToString())
                                .AddAttributionText("米游社每日签到")
                                .Show();
                        }
                    }

                }
            }

            splashView.CurrentStateDescription = "完成";
            //post actions
            if (!navigationService.HasEverNavigated)
            {
                navigationService.Navigate<HomePage>(isSyncTabRequested: true);
            }
            splashView.HasCheckCompleted = true;
        }

        #region Update
        private async Task CheckUpdateAsync()
        {
            UpdateState result = await UpdateService.Instance.CheckUpdateStateAsync();
            switch (result)
            {
                case UpdateState.NeedUpdate:
                    {
                        if (await ShowConfirmUpdateDialogAsync() == ContentDialogResult.Primary)
                        {
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
