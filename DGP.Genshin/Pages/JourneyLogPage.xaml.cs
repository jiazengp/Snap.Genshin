using DGP.Genshin.Services;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// JourneyLogPage.xaml 的交互逻辑
    /// </summary>
    public partial class JourneyLogPage : Page
    {
        public JourneyLogPage()
        {
            this.DataContext = new JourneyLogService();
            InitializeComponent();
        }
    }
}
