using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Cookie;
using DGP.Genshin.Helpers;
using DGP.Genshin.Services.Settings;
using DGP.Genshin.Services.Updating;
using ModernWpf;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Pages
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingModel SettingModel => SettingModel.Instance;
        private AutoRunHelper autoRunHelper = new();

        public SettingsPage()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //version
            Version? v = Assembly.GetExecutingAssembly().GetName().Version;
            if (v is not null)
            {
                VersionString = $"DGP.Genshin - version {v.Major}.{v.Minor}.{v.Build} Build {v.Revision}";
            }

            //theme
            ThemeComboBox.SelectedIndex =
                SettingService.Instance.GetOrDefault(Setting.AppTheme, null, Setting.ApplicationThemeConverter) switch
                {
                    ApplicationTheme.Light => 0,
                    ApplicationTheme.Dark => 1,
                    _ => 2,
                };
            this.Log("initialized");
        }

        #region observable
        public string VersionString
        {
            get => (string)GetValue(VersionStringProperty);
            set => SetValue(VersionStringProperty, value);
        }
        public static readonly DependencyProperty VersionStringProperty =
            DependencyProperty.Register("VersionString", typeof(string), typeof(SettingsPage), new PropertyMetadata(""));
        #endregion

        public AutoRunHelper AutoRunHelper { get => autoRunHelper; set => autoRunHelper = value; }

        private async void UpdateRequestedAsync(object sender, RoutedEventArgs e)
        {
            UpdateState u = await UpdateService.Instance.CheckUpdateStateAsync();

            Button button = ((Button)sender);
            if (u is UpdateState.NeedUpdate)
            {
                UpdateService.Instance.DownloadAndInstallPackage();
                await new UpdateDialog().ShowAsync();
            }
            else
            {
                button.Content = u switch
                {
                    UpdateState.IsNewestRelease => "已是最新版",
                    UpdateState.IsInsiderVersion => "内部测试版",
                    UpdateState.NotAvailable => "获取更新失败",
                    _ => throw new InvalidOperationException("检查更新期间发生未知错误")
                };
            }
        }

        private void ThemeChangeRequested(object sender, SelectionChangedEventArgs e)
        {
            SettingService.Instance[Setting.AppTheme] = ((ComboBox)sender).SelectedIndex switch
            {
                0 => ApplicationTheme.Light,
                1 => ApplicationTheme.Dark,
                _ => null,
            };
            SetAppTheme();
        }
        internal void SetAppTheme()
        {
            ThemeManager.Current.ApplicationTheme =
                SettingService.Instance.GetOrDefault(Setting.AppTheme, null, Setting.ApplicationThemeConverter);
        }

        private async void ResetCookieButtonClick(object sender, RoutedEventArgs e)
        {
            await CookieManager.SetCookieAsync();
        }
    }
}
