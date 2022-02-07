using DGP.Genshin.ViewModel;

namespace DGP.Genshin.Page
{
    /// <summary>
    /// WebViewLobbyPage.xaml 的交互逻辑
    /// </summary>
    public partial class WebViewLobbyPage : System.Windows.Controls.Page
    {
        public WebViewLobbyPage()
        {
            DataContext = App.AutoWired<WebViewLobbyViewModel>();
            InitializeComponent();
        }
    }
}
