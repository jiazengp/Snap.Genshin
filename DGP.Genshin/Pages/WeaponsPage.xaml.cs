using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// WeaponsPage.xaml 的交互逻辑
    /// </summary>
    public partial class WeaponsPage : Page
    {
        public WeaponsPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await Task.Delay(300);
            MetaDataService service = MetaDataService.Instance;
            DataContext = service;
            service.SelectedWeapon ??= service.Weapons?.First();
            this.Log("initialized");
        }
    }
}
