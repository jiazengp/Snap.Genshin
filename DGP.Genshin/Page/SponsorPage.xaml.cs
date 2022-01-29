using DGP.Genshin.ViewModel;

namespace DGP.Genshin.Page
{
    /// <summary>
    /// SponsorPage.xaml 的交互逻辑
    /// </summary>
    public partial class SponsorPage : System.Windows.Controls.Page
    {
        public SponsorPage()
        {
            DataContext = App.AutoWired<SponsorViewModel>();
            InitializeComponent();
        }
    }
}
