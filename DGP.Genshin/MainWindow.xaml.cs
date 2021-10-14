using DGP.Genshin.Controls;
using DGP.Genshin.Controls.Markdown;
using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.Sign;
using DGP.Genshin.Models.MiHoYo.User;
using DGP.Genshin.Models.MiHoYo.UserInfo;
using DGP.Genshin.Pages;
using DGP.Genshin.Services;
using DGP.Genshin.Services.Settings;
using DGP.Genshin.Services.Updating;
using DGP.Snap.Framework.Attributes;
using DGP.Snap.Framework.Extensions.System;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System;
using System.Linq;
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
        private readonly DailySignInService dailySignInService;
        public MainWindow()
        {
            InitializeComponent();
            this.MainSplashView.InitializationPostAction += SplashInitializeCompleted;
            this.navigationService = new NavigationService(this, this.NavView, this.ContentFrame);
            this.dailySignInService = new DailySignInService();
            this.Log("initialized");
            CookieManager.CookieRefreshed += RefreshUserInfoAsync;
        }

        [HandleEvent]
        private async void SplashInitializeCompleted(SplashView splashView)
        {
            splashView.CurrentStateDescription = "检查程序更新...";
            await CheckUpdateAsync();

            splashView.CurrentStateDescription = "初始化用户信息...";
            await InitializeUserInfoAsync();

            //签到
            if (SettingService.Instance.GetOrDefault(Setting.AutoDailySignInOnLaunch, false))
            {
                DateTime time = SettingService.Instance.GetOrDefault(
                    Setting.LastAutoSignInTime,
                    DateTime.Today.AddDays(-1),
                    str => DateTime.Parse((string)str));

                if (time <= DateTime.Today)
                {
                    splashView.CurrentStateDescription = "签到中...";
                    await InitializeSignInPanelDataAsync();
                    SignInResult result = await this.dailySignInService.SignInAsync(this.SelectedRole);
                    new ToastContentBuilder().AddText(result != null ? "签到成功" : "签到失败").Show();
                }
            }

            splashView.CurrentStateDescription = "完成";
            if (!this.navigationService.HasEverNavigated)
            {
                this.navigationService.Navigate<HomePage>(isSyncTabRequested: true);
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
            return await new ContentDialog
            {
                Title = UpdateService.Instance.Release.Name,
                Content = new FlowDocumentScrollViewer
                {
                    Document = new TextToFlowDocumentConverter
                    {
                        Markdown = FindResource("Markdown") as Markdown
                    }.Convert(UpdateService.Instance.Release.Body, typeof(FlowDocument), null, null) as FlowDocument
                },
                PrimaryButtonText = "更新",
                CloseButtonText = "忽略",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
        }
        #endregion

        #region SignIn

        #region Observable
        /// <summary>
        /// 签到奖励一览
        /// </summary>
        public SignInReward SignInReward
        {
            get => (SignInReward)GetValue(SignInRewardProperty);
            set => SetValue(SignInRewardProperty, value);
        }
        public static readonly DependencyProperty SignInRewardProperty =
            DependencyProperty.Register("SignInReward", typeof(SignInReward), typeof(MainWindow), new PropertyMetadata(null));
        /// <summary>
        /// 当前签到状态信息
        /// </summary>
        public SignInInfo SignInInfo
        {
            get => (SignInInfo)GetValue(SignInInfoProperty);
            set => SetValue(SignInInfoProperty, value);
        }
        public static readonly DependencyProperty SignInInfoProperty =
            DependencyProperty.Register("SignInInfo", typeof(SignInInfo), typeof(MainWindow), new PropertyMetadata(null));
        /// <summary>
        /// 绑定的角色信息
        /// </summary>
        public UserGameRoleInfo RoleInfo
        {
            get => (UserGameRoleInfo)GetValue(RoleInfoProperty);
            set => SetValue(RoleInfoProperty, value);
        }
        public static readonly DependencyProperty RoleInfoProperty =
            DependencyProperty.Register("RoleInfo", typeof(UserGameRoleInfo), typeof(MainWindow), new PropertyMetadata(null));
        /// <summary>
        /// 选择的角色
        /// </summary>
        public UserGameRole SelectedRole
        {
            get => (UserGameRole)GetValue(SelectedRoleProperty);
            set => SetValue(SelectedRoleProperty, value);
        }
        public static readonly DependencyProperty SelectedRoleProperty =
            DependencyProperty.Register("SelectedRole", typeof(UserGameRole), typeof(MainWindow), new PropertyMetadata(null, OnSelectedRoleChanged));
        #endregion

        private static async void OnSelectedRoleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainWindow window = (MainWindow)d;
            window.SignInInfo = await window.dailySignInService.GetSignInInfoAsync(window.SelectedRole);
            window.SyncItemsStateWithCurrentInfo();
        }

        private async void SignInTitleBarButtonClick(object sender, RoutedEventArgs e)
        {
            //initialize flyout
            Flyout flyout = FlyoutBase.GetAttachedFlyout((TitleBarButton)sender) as Flyout;
            Grid grid = flyout.Content as Grid;
            grid.DataContext = this;
            FlyoutBase.ShowAttachedFlyout((TitleBarButton)sender);

            await InitializeSignInPanelDataAsync();
        }

        /// <summary>
        /// 更新物品透明度
        /// </summary>
        private void SyncItemsStateWithCurrentInfo()
        {
            for (int i = 0; i < this.SignInReward?.Awards.Count; i++)
            {
                SignInAward item = this.SignInReward.Awards[i];
                item.Opacity = (i + 1) <= this.SignInInfo.TotalSignDay ? 0.2 : 1;
            }
        }
        /// <summary>
        /// 初始化 <see cref="SignInReward"/> 与 <see cref="SignInInfo"/>
        /// </summary>
        /// <returns></returns>
        private async Task InitializeSignInPanelDataAsync()
        {
            if (this.SignInReward == null)
            {
                this.SignInReward = await this.dailySignInService.GetSignInRewardAsync();
            }
            if (this.SignInInfo == null)
            {
                this.RoleInfo = await this.dailySignInService.GetUserGameRolesAsync();
                this.SelectedRole = this.RoleInfo?.List.First();
            }
        }

        private bool isSigningIn = false;
        private async void SignInButtonClick(object sender, RoutedEventArgs e)
        {
            if (!this.isSigningIn)
            {
                this.isSigningIn = true;
                SignInResult result = await this.dailySignInService.SignInAsync(this.SelectedRole);
                new ToastContentBuilder().AddText(result != null ? "签到成功" : "签到失败").Show();
                this.SignInReward = await this.dailySignInService.GetSignInRewardAsync();
                //refresh info
                this.SignInInfo = await this.dailySignInService.GetSignInInfoAsync(this.SelectedRole);
                SyncItemsStateWithCurrentInfo();
                this.isSigningIn = false;
            }
        }
        #endregion

        #region UserInfo
        public UserInfo UserInfo
        {
            get => (UserInfo)GetValue(UserInfoProperty);
            set => SetValue(UserInfoProperty, value);
        }
        public static readonly DependencyProperty UserInfoProperty =
            DependencyProperty.Register("UserInfo", typeof(UserInfo), typeof(MainWindow), new PropertyMetadata(null));
        private async void RefreshUserInfoAsync() =>
            this.UserInfo = await new MiHoYoBBSService().GetUserFullInfoAsync();
        private async Task InitializeUserInfoAsync() =>
            this.UserInfo = await new MiHoYoBBSService().GetUserFullInfoAsync();
        private void UserTitleButtonClick(object sender, RoutedEventArgs e)
        {
            Flyout flyout = FlyoutBase.GetAttachedFlyout((TitleBarButton)sender) as Flyout;
            Grid grid = flyout.Content as Grid;
            grid.DataContext = this;
            FlyoutBase.ShowAttachedFlyout((TitleBarButton)sender);
        }
        #endregion
    }
}
