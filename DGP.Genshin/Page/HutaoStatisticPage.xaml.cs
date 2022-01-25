using DGP.Genshin.ViewModel;

namespace DGP.Genshin.Page
{
    public partial class HutaoStatisticPage : System.Windows.Controls.Page
    {
        public HutaoStatisticPage()
        {
            DataContext = App.GetViewModel<HutaoStatisticViewModel>();
            InitializeComponent();
        }
    }
}
