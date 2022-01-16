using DGP.Genshin.ViewModels;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// SponsorPage.xaml 的交互逻辑
    /// </summary>
    public partial class SponsorPage : Page
    {
        public SponsorPage()
        {
            DataContext = App.GetViewModel<SponsorViewModel>();
            InitializeComponent();
        }
    }
}
