using DGP.Genshin.ViewModel;

namespace DGP.Genshin.Page
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
