using DGP.Genshin.Controls.Markdown;
using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.UserInfo;
using DGP.Genshin.Models.MiHoYo.Sign;
using DGP.Genshin.Models.MiHoYo.User;
using DGP.Genshin.Pages;
using DGP.Genshin.Services;
using DGP.Genshin.Services.Updating;
using DGP.Snap.Framework.Attributes;
using DGP.Snap.Framework.Extensions.System;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Linq;

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
            this.navigationService = new NavigationService(this, this.NavView, this.ContentFrame);
            this.dailySignInService = new DailySignInService();
            this.Log("initialized");
            CookieManager.CookieChanged += RefreshUserInfoAsync;
        }

        [HandleEvent]
        private async void SplashInitializeCompleted()
        {
            await CookieManager.EnsureCookieExistAsync();
            await InitializeUserInfoAsync();
            if (!this.navigationService.HasEverNavigated)
            {
                this.navigationService.Navigate<HomePage>(true);
            }
            await CheckUpdateAsync();
        }
        private async Task CheckUpdateAsync()
        {
            UpdateState result = await UpdateService.Instance.CheckUpdateStateViaGithubAsync();
            if (result == UpdateState.NeedUpdate)
            {
                ContentDialogResult dialogResult = await new ContentDialog
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

                if (dialogResult == ContentDialogResult.Primary)
                {
                    UpdateService.Instance.DownloadAndInstallPackage();
                    await new UpdateDialog().ShowAsync();
                }
            }
        }

        #region SignIn
        public SignInReward SignInReward
        {
            get => (SignInReward)GetValue(SignInRewardProperty);
            set => SetValue(SignInRewardProperty, value);
        }
        public static readonly DependencyProperty SignInRewardProperty =
            DependencyProperty.Register("SignInReward", typeof(SignInReward), typeof(MainWindow), new PropertyMetadata(null));

        public SignInInfo SignInInfo
        {
            get => (SignInInfo)GetValue(SignInInfoProperty);
            set => SetValue(SignInInfoProperty, value);
        }
        public static readonly DependencyProperty SignInInfoProperty =
            DependencyProperty.Register("SignInInfo", typeof(SignInInfo), typeof(MainWindow), new PropertyMetadata(null));

        public UserGameRoleInfo RoleInfo
        {
            get { return (UserGameRoleInfo)GetValue(RoleInfoProperty); }
            set { SetValue(RoleInfoProperty, value); }
        }
        public static readonly DependencyProperty RoleInfoProperty =
            DependencyProperty.Register("RoleInfo", typeof(UserGameRoleInfo), typeof(MainWindow), new PropertyMetadata(null));

        public UserGameRole SelectedRole
        {
            get { return (UserGameRole)GetValue(SelectedRoleProperty); }
            set { SetValue(SelectedRoleProperty, value); }
        }
        public static readonly DependencyProperty SelectedRoleProperty =
            DependencyProperty.Register("SelectedRole", typeof(UserGameRole), typeof(MainWindow), new PropertyMetadata(null));

        private async void SignInTitleBarButtonClick(object sender, RoutedEventArgs e)
        {
            Flyout flyout = FlyoutBase.GetAttachedFlyout((TitleBarButton)sender) as Flyout;
            Grid grid = flyout.Content as Grid;
            grid.DataContext = this;
            FlyoutBase.ShowAttachedFlyout((TitleBarButton)sender);
            await InitializeSignInDataAsync();
            UpdateSignInAwards();
        }

        /// <summary>
        /// 更新物品透明度
        /// </summary>
        private void UpdateSignInAwards()
        {
            for (int i = 0; i < this.SignInReward.Awards.Count; i++)
            {
                SignInAward item = this.SignInReward.Awards[i];
                item.Opacity = (i + 1) <= this.SignInInfo.TotalSignDay ? 0.2 : 1;
            }
        }

        private async Task InitializeSignInDataAsync()
        {
            //no cookie data needed
            if (this.SignInReward == null)
            {
                this.SignInReward = await this.dailySignInService.GetSignInRewardAsync();
            }
            if (this.SignInInfo == null)
            {
                this.RoleInfo = await this.dailySignInService.GetUserGameRolesAsync();
                this.SelectedRole = this.RoleInfo.List.First();
                this.SignInInfo = await this.dailySignInService.GetSignInInfoAsync(this.SelectedRole);
            }
        }

        private bool isSigningIn = false;
        private async void SignInButtonClick(object sender, RoutedEventArgs e)
        {
            if (!this.isSigningIn)
            {
                this.isSigningIn = true;
                SignInResult result = await this.dailySignInService.SignInAsync();
                await new ContentDialog
                {
                    Title = "签到",
                    Content = result != null ? "签到成功" : "签到失败",
                    PrimaryButtonText = "确认",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
                this.SignInReward = await this.dailySignInService.GetSignInRewardAsync();
                //refresh info
                this.SignInInfo = await this.dailySignInService.GetSignInInfoAsync(SelectedRole);
                UpdateSignInAwards();
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
