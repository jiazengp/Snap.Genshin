using System.Windows.Controls;

namespace DGP.Genshin.Sample.Plugin
{
    /// <summary>
    /// SamplePage.xaml 的交互逻辑
    /// </summary>
    public partial class SamplePage : Page
    {
        public SamplePage()
        {
            DataContext = App.GetViewModel<SampleViewModel>();
            InitializeComponent();
        }
    }
}
