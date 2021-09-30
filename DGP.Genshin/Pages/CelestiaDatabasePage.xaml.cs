using DGP.Genshin.Services;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// CelestiaDatabasePage.xaml 的交互逻辑
    /// </summary>
    public partial class CelestiaDatabasePage : Page
    {
        public CelestiaDatabasePage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = CelestiaDatabaseService.Instance;
            await CelestiaDatabaseService.Instance.Initialize();
        }
    }
}
