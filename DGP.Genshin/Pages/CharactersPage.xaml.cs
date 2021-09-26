using DGP.Genshin.Services;
using DGP.Snap.Framework.Extensions.System;
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
            await Task.Delay(1000);
            MetaDataService service = MetaDataService.Instance;
            this.DataContext = service;
            if (service.SelectedCharacter == null)
            {
                service.SelectedCharacter = service.Characters.First();
            }
            this.Log("initialized");
        }
    }
}
