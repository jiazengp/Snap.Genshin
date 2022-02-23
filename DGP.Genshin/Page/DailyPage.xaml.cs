using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    public partial class DailyPage : System.Windows.Controls.Page
    {
        public DailyPage(DailyViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}