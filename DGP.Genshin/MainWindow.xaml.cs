using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Controls;
using DGP.Genshin.Cookie;
using DGP.Genshin.MiHoYoAPI.Sign;
using DGP.Genshin.MiHoYoAPI.User;
using DGP.Genshin.Pages;
using DGP.Genshin.Services;
using DGP.Genshin.Services.Notifications;
using DGP.Genshin.Services.Settings;
using DGP.Genshin.Services.Updating;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

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
            await UserInfoTitleButton.InitializeAsync();

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

        /// <summary>
        /// 现在会对Cookie列表内的所有角色签到
        /// </summary>
        /// <param name="splashView"></param>
        /// <returns></returns>
        private static async Task SignInOnStartUp(SplashView splashView)
        {
            DateTime? converter(object? str) => str is not null ? DateTime.Parse((string)str) : null;
            DateTime? latsSignInTime = SettingService.Instance.GetOrDefault(Setting.LastAutoSignInTime, DateTime.Today.AddDays(-1), converter);

            if (latsSignInTime < DateTime.Today)
            {
                splashView.CurrentStateDescription = "签到中...";
                foreach (string cookie in CookieManager.Cookies)
                {
                    UserGameRoleInfo? roleInfo = await new UserGameRoleProvider(cookie).GetUserGameRolesAsync();
                    List<UserGameRole>? list = roleInfo?.List;
                    if (list is not null)
                    {
                        foreach (UserGameRole role in list)
                        {
                            SignInResult? result = await new SignInProvider(cookie).SignInAsync(role);
                            SettingService.Instance[Setting.LastAutoSignInTime] = DateTime.Now;
                            new ToastContentBuilder()
                                .AddSignInHeader("米游社每日签到")
                                .AddText(role.ToString())
                                .AddText(result is null ? "签到失败" : "签到成功")
                                .Show();
                        }
                    }
                }

            }
        }

        #region Update
        private async Task CheckUpdateAsync()
        {
            UpdateState result = await UpdateService.Instance.CheckUpdateStateAsync();

            if (Debugger.IsAttached) result = UpdateState.NeedUpdate;

            switch (result)
            {
                case UpdateState.NeedUpdate:
                    {
                        new ToastContentBuilder()
                            .AddText("有新的更新可用")
                            .AddText(UpdateService.Instance.NewVersion?.ToString())
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
