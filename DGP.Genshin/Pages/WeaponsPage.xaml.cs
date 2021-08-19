using DGP.Genshin.Services;
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
            DataService service = DataService.Instance;
            this.DataContext = service;
            if (service.SelectedWeapon == null)
            {
                service.SelectedWeapon = service.Weapons[0];
            }
            InitializeComponent();
        }
    }
}
