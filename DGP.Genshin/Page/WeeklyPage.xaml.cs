using DGP.Genshin.ViewModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.Page
{
    /// <summary>
    /// WeeklyPage.xaml 的交互逻辑
    /// </summary>
    public partial class WeeklyPage : System.Windows.Controls.Page
    {
        public WeeklyPage()
        {
            InitializeComponent();
        }

        private async void PageLoadedAsync(object sender, System.Windows.RoutedEventArgs e)
        {
            await Task.Delay(1000);
            DataContext = App.GetViewModel<WeeklyViewModel>();
        }
    }
}
