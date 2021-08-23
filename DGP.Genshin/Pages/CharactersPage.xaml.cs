using DGP.Genshin.Services;
using System.Linq;
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
            MetaDataService service = MetaDataService.Instance;
            this.DataContext = service;
            if (service.SelectedCharacter == null)
            {
                service.SelectedCharacter = service.Characters.First();
            }
            InitializeComponent();
        }
    }
}
