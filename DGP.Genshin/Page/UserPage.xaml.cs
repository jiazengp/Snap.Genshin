using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    internal partial class UserPage : System.Windows.Controls.Page
    {
        public UserPage(UserViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
