using System.Windows.Controls;

namespace DGP.Genshin.Sample.Plugin
{
    public partial class SamplePage : Page
    {
        public SamplePage()
        {
            DataContext = App.GetViewModel<SampleViewModel>();
            InitializeComponent();
        }
    }
}
