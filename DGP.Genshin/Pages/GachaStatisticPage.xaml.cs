using DGP.Genshin.ViewModels;
using ModernWpf.Controls;

namespace DGP.Genshin.Pages
{
    public partial class GachaStatisticPage : Page
    {
        public GachaStatisticPage()
        {
            DataContext = App.GetViewModel<GachaStatisticViewModel>();
            InitializeComponent();
        }
    }
}
