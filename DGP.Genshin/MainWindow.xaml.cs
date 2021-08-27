using DGP.Genshin.Controls.Markdown;
using DGP.Genshin.Models.MiHoYo;
using DGP.Genshin.Models.MiHoYo.BBSAPI;
using DGP.Genshin.Pages;
using DGP.Genshin.Services;
using DGP.Genshin.Services.Updating;
using DGP.Snap.Framework.Attributes;
using DGP.Snap.Framework.Extensions.System;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System.Collections.Generic;
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

        public MainWindow()
        {
            InitializeComponent();
            this.navigationService = new NavigationService(this, this.NavView, this.ContentFrame);
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

        private bool isSigningIn = false;
        private async void SignInTitleBarButtonClick(object sender, RoutedEventArgs e)
        {
            if (!this.isSigningIn)
            {
                this.isSigningIn = true;
                List<Models.MiHoYo.Result> results = await Task.Run(async () => await new DailySignInService().SignInAsync());
                bool finalResult = results.Exists(r => r.ReturnCode != 0);
                await new ContentDialog
                {
                    Title = "签到",
                    Content = results.Last().Message,
                    PrimaryButtonText = "确认",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
                this.isSigningIn = false;
            }
        }

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
    }
}
