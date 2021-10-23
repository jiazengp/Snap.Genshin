using DGP.Genshin.Controls;
using DGP.Genshin.Controls.Infrastructures.Markdown;
using DGP.Genshin.Cookie;
using DGP.Genshin.Models.MiHoYo.Sign;
using DGP.Genshin.Models.MiHoYo.User;
using DGP.Genshin.Models.MiHoYo.UserInfo;
using DGP.Genshin.Pages;
using DGP.Genshin.Services;
using DGP.Genshin.Services.Settings;
using DGP.Genshin.Services.Updating;
using DGP.Snap.Framework.Extensions.System;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System;
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
        private readonly DailyNoteService dailyNoteService;
        public MainWindow()
        {
            //never set datacontext for mainwindow
            InitializeComponent();
            MainSplashView.InitializationPostAction += SplashInitializeCompleted;
            CookieManager.CookieRefreshed += RefreshUserInfoAsync;

            navigationService = new NavigationService(this, NavView, ContentFrame);
            dailySignInService = new DailySignInService();
            dailyNoteService = new DailyNoteService();

            this.Log("initialized");
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
                DateTime? time = SettingService.Instance.GetOrDefault(
                    Setting.LastAutoSignInTime,
                    DateTime.Today.AddDays(-1),
                    str => str is not null ? DateTime.Parse((string)str) : (DateTime?)null);

                if (time <= DateTime.Today)
                {
                    splashView.CurrentStateDescription = "签到中...";
                    await InitializeSignInPanelDataAsync();
                    SignInResult? result = await dailySignInService.SignInAsync(SelectedRole);
                    new ToastContentBuilder().AddText(result is not null ? "签到成功" : "签到失败").Show();
                }
            }

            splashView.CurrentStateDescription = "完成";
            //post actions
            await dailyNoteService.RefreshAsync();
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

        #region SignIn

        #region Observable
        /// <summary>
        /// 签到奖励一览
        /// </summary>
        public SignInReward? SignInReward
        {
            get => (SignInReward)GetValue(SignInRewardProperty);
            set => SetValue(SignInRewardProperty, value);
        }
        public static readonly DependencyProperty SignInRewardProperty =
            DependencyProperty.Register("SignInReward", typeof(SignInReward), typeof(MainWindow), new PropertyMetadata(null));
        /// <summary>
        /// 当前签到状态信息
        /// </summary>
        public SignInInfo? SignInInfo
        {
            get => (SignInInfo)GetValue(SignInInfoProperty);
            set => SetValue(SignInInfoProperty, value);
        }
        public static readonly DependencyProperty SignInInfoProperty =
            DependencyProperty.Register("SignInInfo", typeof(SignInInfo), typeof(MainWindow), new PropertyMetadata(null));
        /// <summary>
        /// 绑定的角色信息
        /// </summary>
        public UserGameRoleInfo? RoleInfo
        {
            get => (UserGameRoleInfo)GetValue(RoleInfoProperty);
            set => SetValue(RoleInfoProperty, value);
        }
        public static readonly DependencyProperty RoleInfoProperty =
            DependencyProperty.Register("RoleInfo", typeof(UserGameRoleInfo), typeof(MainWindow), new PropertyMetadata(null));
        /// <summary>
        /// 选择的角色
        /// </summary>
        public UserGameRole? SelectedRole
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
            Flyout? flyout = FlyoutBase.GetAttachedFlyout((TitleBarButton)sender) as Flyout;
            Debug.Assert(flyout is not null);
            Grid? grid = flyout.Content as Grid;
            Debug.Assert(grid is not null);
            grid.DataContext = this;
            FlyoutBase.ShowAttachedFlyout((TitleBarButton)sender);

            await InitializeSignInPanelDataAsync();
        }

        /// <summary>
        /// 更新物品透明度
        /// </summary>
        private void SyncItemsStateWithCurrentInfo()
        {
            for (int i = 0; i < SignInReward?.Awards?.Count; i++)
            {
                SignInAward item = SignInReward.Awards[i];
                item.Opacity = (i + 1) <= SignInInfo?.TotalSignDay ? 0.2 : 1;
            }
        }
        /// <summary>
        /// 初始化 <see cref="SignInReward"/> 与 <see cref="SignInInfo"/>
        /// </summary>
        /// <returns></returns>
        private async Task InitializeSignInPanelDataAsync()
        {
            if (SignInReward == null)
            {
                SignInReward = await dailySignInService.GetSignInRewardAsync();
            }
            if (SignInInfo == null)
            {
                RoleInfo = await dailySignInService.GetUserGameRolesAsync();
                SelectedRole = RoleInfo?.List?.First();
            }
        }

        private bool isSigningIn = false;
        private async void SignInButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isSigningIn)
            {
                isSigningIn = true;
                SignInResult? result = await dailySignInService.SignInAsync(SelectedRole);
                new ToastContentBuilder().AddText(result is not null ? "签到成功" : "签到失败").Show();
                SignInReward = await dailySignInService.GetSignInRewardAsync();
                //refresh info
                SignInInfo = await dailySignInService.GetSignInInfoAsync(SelectedRole);
                SyncItemsStateWithCurrentInfo();
                isSigningIn = false;
            }
        }
        #endregion

        #region UserInfo
        public UserInfo? UserInfo
        {
            get => (UserInfo)GetValue(UserInfoProperty);
            set => SetValue(UserInfoProperty, value);
        }
        public static readonly DependencyProperty UserInfoProperty =
            DependencyProperty.Register("UserInfo", typeof(UserInfo), typeof(MainWindow), new PropertyMetadata(null));
        private async void RefreshUserInfoAsync()
        {
            UserInfo = await new MiHoYoBBSService().GetUserFullInfoAsync();
        }

        private async Task InitializeUserInfoAsync()
        {
            UserInfo = await new MiHoYoBBSService().GetUserFullInfoAsync();
        }

        private void UserTitleButtonClick(object sender, RoutedEventArgs e)
        {
            Flyout? flyout = FlyoutBase.GetAttachedFlyout((TitleBarButton)sender) as Flyout;
            Debug.Assert(flyout is not null);
            Grid? grid = flyout.Content as Grid;
            Debug.Assert(grid is not null);
            grid.DataContext = this;
            FlyoutBase.ShowAttachedFlyout((TitleBarButton)sender);
        }
        #endregion

        #region DailyNote
        private async void DailyNoteTitleBarButtonClick(object sender, RoutedEventArgs e)
        {
            await dailyNoteService.RefreshAsync();

            Flyout? flyout = FlyoutBase.GetAttachedFlyout((TitleBarButton)sender) as Flyout;
            Debug.Assert(flyout is not null);
            Grid? grid = flyout.Content as Grid;
            Debug.Assert(grid is not null);
            grid.DataContext = dailyNoteService;
            FlyoutBase.ShowAttachedFlyout((TitleBarButton)sender);
        }
        #endregion
    }
}
