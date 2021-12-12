using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// WeaponsPage.xaml 的交互逻辑
    /// </summary>
    public partial class WeaponsPage : ModernWpf.Controls.Page
    {
        public WeaponsPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            MetadataViewModel service = MetadataViewModel.Instance;
            await Task.Delay(300);
            DataContext = service;
            service.SelectedWeapon ??= service.Weapons?.First();
            this.Log("initialized");
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DataContext = null;
            this.Log("uninitialized");
            GC.Collect(GC.MaxGeneration);
            base.OnNavigatedFrom(e);
        }

        private void FilterToggleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MetadataViewModel.Instance.FilterCharacterAndWeapon();
        }
    }
}
