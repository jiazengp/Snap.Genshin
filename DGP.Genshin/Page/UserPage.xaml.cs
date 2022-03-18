using DGP.Genshin.Control.Infrastructure.Concurrent;
using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    internal partial class UserPage : AsyncPage
    {
        public UserPage(UserViewModel vm) : base(vm)
        {
            InitializeComponent();
        }
    }
}
