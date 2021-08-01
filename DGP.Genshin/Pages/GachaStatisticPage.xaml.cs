using DGP.Genshin.Services.GachaStatistic;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// GachaStatisticPage.xaml 的交互逻辑
    /// </summary>
    public partial class GachaStatisticPage : Page
    {
        public GachaStatisticPage()
        {
            this.DataContext = GachaStatisticService.Instance;
            GachaStatisticService.Instance.Initialize();
            this.InitializeComponent();
        }

        private void RefreshAppBarButtonClick(object sender, System.Windows.RoutedEventArgs e) => GachaStatisticService.Instance.Refresh();
    }
}
