using DGP.Genshin.Services;
using DGP.Snap.Framework.Core.LifeCycling;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            this.DataContext = LifeCycle.InstanceOf<DailyViewService>();
            this.InitializeComponent();
        }
    }
}