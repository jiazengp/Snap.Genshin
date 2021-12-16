using DGP.Genshin.ViewModels;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// WeeklyPage.xaml 的交互逻辑
    /// </summary>
    public partial class WeeklyPage : Page
    {
        public WeeklyPage()
        {
            DataContext = App.GetViewModel<WeeklyViewModel>();
            InitializeComponent();
        }
    }
}
