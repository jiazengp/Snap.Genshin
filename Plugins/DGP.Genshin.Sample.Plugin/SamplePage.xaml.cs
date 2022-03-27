using Snap.Core.DependencyInjection;
using SystemPage = System.Windows.Controls.Page;

namespace DGP.Genshin.Sample.Plugin
{
    [View(InjectAs.Transient)]
    public partial class SamplePage : SystemPage
    {
        public SamplePage(SampleViewModel vm)
        {
            this.DataContext = vm;
            this.InitializeComponent();
        }
    }
}
