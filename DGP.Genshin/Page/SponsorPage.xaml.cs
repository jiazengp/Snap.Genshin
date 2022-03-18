using DGP.Genshin.Control.Infrastructure.Concurrent;
using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    internal partial class SponsorPage : AsyncPage
    {
        public SponsorPage(SponsorViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
