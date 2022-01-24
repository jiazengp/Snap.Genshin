using DGP.Genshin.ViewModel;

namespace DGP.Genshin.Page
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
