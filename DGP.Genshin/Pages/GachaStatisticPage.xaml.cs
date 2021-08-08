using DGP.Genshin.Services.GachaStatistic;
using ModernWpf.Controls;
using System.Windows.Navigation;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// GachaStatisticPage.xaml 的交互逻辑
    /// </summary>
    public partial class GachaStatisticPage : Page
    {
        private GachaStatisticService Service { get; set; }

        public GachaStatisticPage()
        {
            this.Service = new GachaStatisticService();
            this.DataContext = this.Service;
            this.InitializeComponent();
        }

        private void RefreshAppBarButtonClick(object sender, System.Windows.RoutedEventArgs e) => this.Service.Refresh();

        private async void ExportAppBarButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.Service.ExportDataToExcelAsync();
            await new ContentDialog
            {
                Title = "导出祈愿记录完成",
                Content = $"请查看桌面上的 {this.Service.SelectedUid}.xlsx 文件",
                PrimaryButtonText = "确定",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Service.UnInitialize();
            base.OnNavigatedFrom(e);
        }
    }
}
