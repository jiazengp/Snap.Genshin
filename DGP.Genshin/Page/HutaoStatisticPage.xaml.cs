using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    internal partial class HutaoStatisticPage : System.Windows.Controls.Page
    {
        public HutaoStatisticPage(HutaoStatisticViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
