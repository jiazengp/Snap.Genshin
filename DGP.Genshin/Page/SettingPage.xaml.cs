using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    internal partial class SettingPage : System.Windows.Controls.Page
    {
        public SettingPage(SettingViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
