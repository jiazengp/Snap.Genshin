using DGP.Genshin.ViewModels;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            DataContext = App.GetViewModel<SettingViewModel>();
            InitializeComponent();
        }

        public SettingViewModel ViewModel => (SettingViewModel)DataContext;
    }
}
