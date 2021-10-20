using DGP.Genshin.Services;
using DGP.Snap.Framework.Extensions.System;
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
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await Task.Delay(1000);
            MetaDataService service = MetaDataService.Instance;
            this.DataContext = service;
            if (service.SelectedWeapon == null)
            {
                service.SelectedWeapon = service.Weapons[0];
            }
            this.Log("initialized");
        }
    }
}
