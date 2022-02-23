using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    public partial class PluginPage : System.Windows.Controls.Page
    {
        public PluginPage(PluginViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
