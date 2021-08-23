using DGP.Genshin.Services;
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
            this.DataContext = WeeklyViewService.Instance;
            InitializeComponent();
        }
    }
}
