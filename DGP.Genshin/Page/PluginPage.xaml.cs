using DGP.Genshin.ViewModel;

namespace DGP.Genshin.Page
{
    /// <summary>
    /// PluginPage.xaml 的交互逻辑
    /// </summary>
    public partial class PluginPage : System.Windows.Controls.Page
    {
        public PluginPage()
        {
            DataContext = App.AutoWired<PluginViewModel>();
            InitializeComponent();
        }
    }
}
