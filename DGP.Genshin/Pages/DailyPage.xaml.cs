using DGP.Genshin.Services;
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
            this.DataContext = DailyViewService.Instance;
            InitializeComponent();
        }
    }
}