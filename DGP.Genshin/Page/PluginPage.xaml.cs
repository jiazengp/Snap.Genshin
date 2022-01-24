using DGP.Genshin.ViewModel;
using System.Windows.Controls;

namespace DGP.Genshin.Page
{
    /// <summary>
    /// PluginPage.xaml 的交互逻辑
    /// </summary>
    public partial class PluginPage : System.Windows.Controls.Page
    {
        public PluginPage()
        {
            DataContext = App.GetViewModel<PluginViewModel>();
            InitializeComponent();
        }
    }
}
