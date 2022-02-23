using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;
using System.Threading.Tasks;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    public partial class WeeklyPage : System.Windows.Controls.Page
    {
        public WeeklyPage(WeeklyViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
