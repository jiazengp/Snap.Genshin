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
        private readonly DailyNoteService dailyNoteService;
        public MainWindow()
        {
            //never set datacontext for mainwindow
            this.InitializeComponent();
            this.MainSplashView.InitializationPostAction += this.SplashInitializeCompleted;
            this.navigationService = new NavigationService(this, this.NavView, this.ContentFrame);
            this.dailySignInService = new DailySignInService();
            this.dailyNoteService = new DailyNoteService();

            CookieManager.CookieRefreshed += this.RefreshUserInfoAsync;

            this.Log("initialized");
        }

        [HandleEvent]
        private async void SplashInitializeCompleted(SplashView splashView)
        {
            splashView.CurrentStateDescription = "检查程序更新...";
            await this.CheckUpdateAsync();

            splashView.CurrentStateDescription = "初始化用户信息...";
            await this.InitializeUserInfoAsync();

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
                    await this.InitializeSignInPanelDataAsync();
                    SignInResult result = await this.dailySignInService.SignInAsync(this.SelectedRole);
                    new ToastContentBuilder().AddText(result != null ? "签到成功" : "签到失败").Show();
                }
            }

            splashView.CurrentStateDescription = "完成";
            //post actions
            await this.dailyNoteService.RefreshAsync();
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
                        if (await this.ShowConfirmUpdateDialogAsync() == ContentDialogResult.Primary)
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
                        Markdown = this.FindResource("Markdown") as Markdown
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
            get => (SignInReward)this.GetValue(SignInRewardProperty);
            set => this.SetValue(SignInRewardProperty, value);
        }
        public static readonly DependencyProperty SignInRewardProperty =
            DependencyProperty.Register("SignInReward", typeof(SignInReward), typeof(MainWindow), new PropertyMetadata(null));
        /// <summary>
        /// 当前签到状态信息
        /// </summary>
        public SignInInfo SignInInfo
        {
            get => (SignInInfo)this.GetValue(SignInInfoProperty);
            set => this.SetValue(SignInInfoProperty, value);
        }
        public static readonly DependencyProperty SignInInfoProperty =
            DependencyProperty.Register("SignInInfo", typeof(SignInInfo), typeof(MainWindow), new PropertyMetadata(null));
        /// <summary>
        /// 绑定的角色信息
        /// </summary>
        public UserGameRoleInfo RoleInfo
        {
            get => (UserGameRoleInfo)this.GetValue(RoleInfoProperty);
            set => this.SetValue(RoleInfoProperty, value);
        }
        public static readonly DependencyProperty RoleInfoProperty =
            DependencyProperty.Register("RoleInfo", typeof(UserGameRoleInfo), typeof(MainWindow), new PropertyMetadata(null));
        /// <summary>
        /// 选择的角色
        /// </summary>
        public UserGameRole SelectedRole
        {
            get => (UserGameRole)this.GetValue(SelectedRoleProperty);
            set => this.SetValue(SelectedRoleProperty, value);
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

            await this.InitializeSignInPanelDataAsync();
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
                this.SyncItemsStateWithCurrentInfo();
                this.isSigningIn = false;
            }
        }
        #endregion

        #region UserInfo
        public UserInfo UserInfo
        {
            get => (UserInfo)this.GetValue(UserInfoProperty);
            set => this.SetValue(UserInfoProperty, value);
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

        #region DailyNote
        private async void DailyNoteTitleBarButtonClick(object sender, RoutedEventArgs e)
        {
            await this.dailyNoteService.RefreshAsync();

            Flyout flyout = FlyoutBase.GetAttachedFlyout((TitleBarButton)sender) as Flyout;
            Grid grid = flyout.Content as Grid;
            grid.DataContext = this.dailyNoteService;
            FlyoutBase.ShowAttachedFlyout((TitleBarButton)sender);
        }
        #endregion
    }
}
