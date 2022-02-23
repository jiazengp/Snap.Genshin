using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    public partial class GachaStatisticPage : ModernWpf.Controls.Page
    {
        public GachaStatisticPage(GachaStatisticViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
