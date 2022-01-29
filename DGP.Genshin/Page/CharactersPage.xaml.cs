using DGP.Genshin.ViewModel;

namespace DGP.Genshin.Page
{
    /// <summary>
    /// CharactersPage.xaml 的交互逻辑
    /// </summary>
    public partial class CharactersPage : ModernWpf.Controls.Page
    {
        public CharactersPage()
        {
            DataContext = App.AutoWired<MetadataViewModel>();
            InitializeComponent();
        }
    }
}
