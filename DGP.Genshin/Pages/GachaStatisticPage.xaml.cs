using DGP.Genshin.Services.GachaStatistic;
using System.Windows.Controls;

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
            //GachaStatisticService.Initialize();
            this.InitializeComponent();
        }

        private void RefreshAppBarButtonClick(object sender, System.Windows.RoutedEventArgs e) => this.Service.Refresh();
    }
}
