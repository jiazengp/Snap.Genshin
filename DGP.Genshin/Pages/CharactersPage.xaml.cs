using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// CharactersPage.xaml 的交互逻辑
    /// </summary>
    public partial class CharactersPage : Page
    {
        public CharactersPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            await Task.Delay(300);
            MetaDataService service = MetaDataService.Instance;
            DataContext = service;
            service.SelectedCharacter ??= service.Characters?.First();
            this.Log("initialized");
        }
    }
}
