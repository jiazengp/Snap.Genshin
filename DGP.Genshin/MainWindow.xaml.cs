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
                    Content = new FlowDocumentScrollViewer
                    {
                        Document = new TextToFlowDocumentConverter
                        {
                            Markdown = this.FindResource("Markdown") as Markdown
                        }.Convert(UpdateService.Instance.ReleaseInfo.Body, typeof(FlowDocument), null, null) as FlowDocument
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

        private async void SignInTitleBarButtonClick(object sender, RoutedEventArgs e)
        {
            List<Models.MiHoYo.Result> results = await new DailySignInService().SignInAsync();
            var finalResult = results.Exists(r => r.ReturnCode != 0);
            await new ContentDialog
            {
                Title = finalResult ? "签到失败" : "签到成功",
                Content = new ModernWpf.Controls.ListView { ItemsSource = results },
                PrimaryButtonText = "确认",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
        }
    }
}
