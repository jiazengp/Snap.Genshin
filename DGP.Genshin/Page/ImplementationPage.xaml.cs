using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    internal partial class ImplementationPage : System.Windows.Controls.Page
    {
        public ImplementationPage()
        {
            this.DataContext = App.Current.SwitchableImplementationManager;
            this.InitializeComponent();
        }
    }
}
