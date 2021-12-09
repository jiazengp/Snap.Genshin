using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// CharactersPage.xaml 的交互逻辑
    /// </summary>
    public partial class CharactersPage : ModernWpf.Controls.Page
    {
        public CharactersPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            MetadataService service = MetadataService.Instance;
            await Task.Delay(300);
            DataContext = service;
            service.SelectedCharacter ??= service.Characters?.First();
            this.Log("initialized");
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DataContext = null;
            this.Log("uninitialized");
            base.OnNavigatedFrom(e);
        }

        private void FilterToggleButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MetadataService.Instance.FilterCharacterAndWeapon();
        }

        private void GachaSplashButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            new CharacterGachaSplashWindow()
            {
                Source = MetadataService.Instance.SelectedCharacter?.GachaSplash,
                Owner = App.Current.MainWindow
            }.ShowDialog();
        }
    }
}
