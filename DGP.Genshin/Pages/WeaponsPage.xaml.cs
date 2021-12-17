using DGP.Genshin.ViewModels;

namespace DGP.Genshin.Pages
{
    public partial class WeaponsPage : ModernWpf.Controls.Page
    {
        public WeaponsPage()
        {
            DataContext = App.GetViewModel<MetadataViewModel>();
            InitializeComponent();
        }
    }
}
