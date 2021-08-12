using DGP.Genshin.Pages;
using DGP.Genshin.Services;
using DGP.Genshin.Services.Updating;
using DGP.Snap.Framework.Attributes;
using ModernWpf.Controls;
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
            this.InitializeComponent();
            this.navigationService = new NavigationService(this, this.NavView, this.ContentFrame);
        }

        [HandleEvent]
        private async void SplashInitializeCompleted()
        {
            this.navigationService.Navigate<HomePage>(true);
            //check for update
            UpdateState result = await Task.Run(UpdateService.Instance.CheckUpdateStateViaGithub);
            if (result == UpdateState.NeedUpdate)
            {
                ContentDialogResult dialogResult = await new ContentDialog
                {
                    Title = UpdateService.Instance.ReleaseInfo.Name,
                    Content = UpdateService.Instance.ReleaseInfo.Body,
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
    }
}
