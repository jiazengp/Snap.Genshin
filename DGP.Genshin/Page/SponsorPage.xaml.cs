using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    public partial class SponsorPage : System.Windows.Controls.Page
    {
        public SponsorPage(SponsorViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
