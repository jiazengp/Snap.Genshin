using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    internal partial class WeeklyPage : System.Windows.Controls.Page
    {
        public WeeklyPage(WeeklyViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
