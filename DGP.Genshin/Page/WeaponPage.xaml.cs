using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    internal partial class WeaponPage : ModernWpf.Controls.Page
    {
        public WeaponPage(MetadataViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
