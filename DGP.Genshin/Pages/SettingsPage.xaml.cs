using DGP.Genshin.Services.Abstratcions;
using DGP.Genshin.ViewModels;
using ModernWpf;
using System.Windows;
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //theme
            ThemeComboBox.SelectedIndex =
                ViewModel.SettingService.GetOrDefault(Setting.AppTheme, null, Setting.ApplicationThemeConverter) switch
                {
                    ApplicationTheme.Light => 0,
                    ApplicationTheme.Dark => 1,
                    _ => 2,
                };
        }
    }
}
