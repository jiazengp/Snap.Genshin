using DGP.Genshin.ViewModel;
using System.Windows.Controls;

namespace DGP.Genshin.Page
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : System.Windows.Controls.Page
    {
        public SettingsPage()
        {
            DataContext = App.GetViewModel<SettingViewModel>();
            InitializeComponent();
        }

        public SettingViewModel ViewModel => (SettingViewModel)DataContext;
    }
}
