using DGP.Genshin.Mate.Services;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Mate.Views
{
    /// <summary>
    /// ResinView.xaml 的交互逻辑
    /// </summary>
    public partial class ResinView : UserControl
    {
        public ResinView()
        {
            DataContext = ResinService.Instance;
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await ResinService.Instance.RefreshAsync();
        }
    }
}
