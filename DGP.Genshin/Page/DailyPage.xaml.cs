using DGP.Genshin.ViewModel;
using System.Windows.Controls;

namespace DGP.Genshin.Page
{
    /// <summary>
    /// DailyPage.xaml 的交互逻辑
    /// </summary>
    public partial class DailyPage : System.Windows.Controls.Page
    {
        public DailyPage()
        {
            DataContext = App.GetViewModel<DailyViewModel>();
            InitializeComponent();
        }
    }
}