using DGP.Genshin.Controls.Markdown;
using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.BBSAPI;
using DGP.Genshin.Models.MiHoYo.Sign;
using DGP.Genshin.Models.MiHoYo.User;
using DGP.Genshin.Pages;
using DGP.Genshin.Services;
using DGP.Genshin.Services.Updating;
using DGP.Snap.Framework.Attributes;
using DGP.Snap.Framework.Extensions.System;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
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
            get { return (SignInInfo)GetValue(SignInInfoProperty); }
            set { SetValue(SignInInfoProperty, value); }
        }
        public static readonly DependencyProperty SignInInfoProperty =
            DependencyProperty.Register("SignInInfo", typeof(SignInInfo), typeof(MainWindow), new PropertyMetadata(null));


        private bool isSigningIn = false;
        private async void SignInTitleBarButtonClick(object sender, RoutedEventArgs e)
        {
            Flyout flyout = FlyoutBase.GetAttachedFlyout((TitleBarButton)sender) as Flyout;
            Grid grid = flyout.Content as Grid;
            grid.DataContext = this;
            FlyoutBase.ShowAttachedFlyout((TitleBarButton)sender);
            await InitializeSignInDataAsync();
            UpdateSignInAwards();
        }

        private void UpdateSignInAwards()
        {
            for (int i = 0; i < SignInReward.Awards.Count; i++)
            {
                SignInAward item = SignInReward.Awards[i];
                item.Opacity = (i + 1) <= SignInInfo.TotalSignDay ? 0.2 : 1;
            }
        }

        private async Task InitializeSignInDataAsync()
        {
            if (this.SignInReward == null)
            {
                this.SignInReward = await dailySignInService.GetSignInRewardAsync();
            }
            if (this.SignInInfo == null)
            {
                UserGameRoleInfo info = await dailySignInService.GetUserGameRolesAsync();
                Debug.Assert(info.List.Count == 1);
                this.SignInInfo = await dailySignInService.GetSignInInfoAsync(info.List[0]);
            }
        }

        private async void SignInButtonClick(object sender, RoutedEventArgs e)
        {
            if (!this.isSigningIn)
            {
                this.isSigningIn = true;
                SignInResult result = await dailySignInService.SignInAsync();
                await new ContentDialog
                {
                    Title = "签到",
                    Content = result != null ? "签到成功" : "签到失败",
                    PrimaryButtonText = "确认",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
                this.SignInReward = await dailySignInService.GetSignInRewardAsync();
                UserGameRoleInfo info = await dailySignInService.GetUserGameRolesAsync();
                Debug.Assert(info.List.Count == 1);
                this.SignInInfo = await dailySignInService.GetSignInInfoAsync(info.List[0]);
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
