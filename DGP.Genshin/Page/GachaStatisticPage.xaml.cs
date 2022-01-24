using DGP.Genshin.ViewModel;
using ModernWpf.Controls;

namespace DGP.Genshin.Page
{
    public partial class GachaStatisticPage : ModernWpf.Controls.Page
    {
        public GachaStatisticPage()
        {
            DataContext = App.GetViewModel<GachaStatisticViewModel>();
            InitializeComponent();
        }
    }
}
