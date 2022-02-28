using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    public partial class LaunchPage : System.Windows.Controls.Page
    {
        public LaunchPage(LaunchViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
