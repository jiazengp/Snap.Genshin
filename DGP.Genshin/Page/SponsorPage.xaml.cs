using DGP.Genshin.ViewModel;
using System.Windows.Controls;

namespace DGP.Genshin.Page
{
    /// <summary>
    /// SponsorPage.xaml 的交互逻辑
    /// </summary>
    public partial class SponsorPage : System.Windows.Controls.Page
    {
        public SponsorPage()
        {
            DataContext = App.GetViewModel<SponsorViewModel>();
            InitializeComponent();
        }
    }
}
