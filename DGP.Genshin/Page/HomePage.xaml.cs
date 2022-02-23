using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    public partial class HomePage : System.Windows.Controls.Page
    {
        public HomePage(HomeViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
