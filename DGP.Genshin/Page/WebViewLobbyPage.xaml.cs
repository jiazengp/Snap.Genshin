using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    internal partial class WebViewLobbyPage : System.Windows.Controls.Page
    {
        public WebViewLobbyPage(WebViewLobbyViewModel vm)
        {
            this.DataContext = vm;
            this.InitializeComponent();
        }
    }
}
