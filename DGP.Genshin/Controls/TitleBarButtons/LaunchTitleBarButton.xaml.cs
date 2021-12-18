using DGP.Genshin.ViewModels;
using ModernWpf.Controls.Primitives;

namespace DGP.Genshin.Controls.TitleBarButtons
{
    /// <summary>
    /// LaunchTitleBarButton.xaml 的交互逻辑
    /// </summary>
    public partial class LaunchTitleBarButton : TitleBarButton
    {
        public LaunchTitleBarButton()
        {
            DataContext = App.GetViewModel<LaunchViewModel>();
            InitializeComponent();
        }

        public LaunchViewModel ViewModel => (LaunchViewModel)DataContext;

        private void Flyout_Closed(object sender, object e)
        {
            ViewModel.CloseUICommand.Execute(null);
        }
    }
}
