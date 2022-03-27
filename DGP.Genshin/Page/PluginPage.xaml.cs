using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    internal partial class PluginPage : System.Windows.Controls.Page
    {
        public PluginPage(PluginViewModel vm)
        {
            this.DataContext = vm;
            this.InitializeComponent();
        }
    }
}
