using DGP.Genshin.Controls.Markdown;
using DGP.Genshin.Pages;
using DGP.Genshin.Services;
using DGP.Genshin.Services.Updating;
using DGP.Snap.Framework.Attributes;
using ModernWpf.Controls;
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
        }


        [HandleEvent]
        private async void SplashInitializeCompleted()
        {
            this.navigationService.Navigate<HomePage>(true);
            //check for update
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
    }
}
