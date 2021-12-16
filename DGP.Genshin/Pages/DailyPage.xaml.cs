using DGP.Genshin.ViewModels;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// DailyPage.xaml 的交互逻辑
    /// </summary>
    public partial class DailyPage : Page
    {
        public DailyPage()
        {
            DataContext = App.GetViewModel<DailyViewModel>();
            InitializeComponent();
        }
    }
}