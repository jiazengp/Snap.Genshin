using DGP.Genshin.Resin.Plugin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Resin.Plugin
{
    [View(InjectAs.Transient)]
    public partial class ResinConfigPage : System.Windows.Controls.Page
    {
        public ResinConfigPage(ResinConfigViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
