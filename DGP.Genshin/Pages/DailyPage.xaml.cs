using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Services;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// DailyPage.xaml 的交互逻辑
    /// </summary>
    public partial class DailyPage : Page
    {
        public DailyPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await Task.Delay(1000);
            DataContext = DailyViewService.Instance;
            this.Log("initialized");
        }
    }
}