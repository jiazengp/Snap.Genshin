using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    internal partial class CharacterPage : ModernWpf.Controls.Page
    {
        public CharacterPage(MetadataViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
