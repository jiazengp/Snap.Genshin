using DGP.Genshin.ViewModel;

namespace DGP.Genshin.Page
{
    public partial class GachaStatisticPage : ModernWpf.Controls.Page
    {
        public GachaStatisticPage()
        {
            DataContext = App.AutoWired<GachaStatisticViewModel>();
            InitializeComponent();
        }
    }
}
