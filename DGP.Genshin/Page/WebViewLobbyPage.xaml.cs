using DGP.Genshin.ViewModel;
using Snap.Core.DependencyInjection;

namespace DGP.Genshin.Page
{
    [View(InjectAs.Transient)]
    public partial class WebViewLobbyPage : System.Windows.Controls.Page
    {
        public WebViewLobbyPage(WebViewLobbyViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
        }
    }
}
