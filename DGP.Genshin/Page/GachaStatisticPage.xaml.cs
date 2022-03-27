using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    internal partial class GachaStatisticPage : ModernWpf.Controls.Page
    {
        public GachaStatisticPage(GachaStatisticViewModel vm)
        {
            this.DataContext = vm;
            this.InitializeComponent();
        }
    }
}
