using DGP.Genshin.Services;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// CharactersPage.xaml 的交互逻辑
    /// </summary>
    public partial class CharactersPage : ModernWpf.Controls.Page
    {
        public CharactersPage()
        {
            DataContext = App.GetViewModel<MetadataViewModel>();
            InitializeComponent();
        }
    }
}
