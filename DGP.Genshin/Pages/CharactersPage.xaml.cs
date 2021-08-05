using DGP.Genshin.Services;
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
            DataService service = DataService.Instance;
            this.DataContext = service;
            if (service.SelectedCharacter == null)
            {
                service.SelectedCharacter = service.Characters[0];
            }
            this.InitializeComponent();
        }
    }
}
