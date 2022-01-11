using DGP.Genshin.ViewModels;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// TO-DO:
    /// 实现Post的刷新
    /// </summary>
    public partial class HomePage : System.Windows.Controls.Page
    {
        public HomePage()
        {
            DataContext = App.GetViewModel<HomeViewModel>();
            InitializeComponent();
        }
    }
}
