using DGP.Genshin.Services;

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
