using DGP.Genshin.ViewModels;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// PluginPage.xaml 的交互逻辑
    /// </summary>
    public partial class PluginPage : Page
    {
        public PluginPage()
        {
            DataContext = App.GetViewModel<PluginViewModel>();
            InitializeComponent();
        }
    }
}
