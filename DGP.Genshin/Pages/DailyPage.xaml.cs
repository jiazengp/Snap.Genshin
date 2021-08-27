using DGP.Genshin.Services;
using DGP.Snap.Framework.Extensions.System;
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
            this.Log("initialized");
        }
    }
}