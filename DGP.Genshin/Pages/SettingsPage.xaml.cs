using DGP.Genshin.Cookie;
using DGP.Genshin.Helpers;
using DGP.Genshin.Services.Settings;
using DGP.Genshin.Services.Updating;
using DGP.Snap.Framework.Extensions.System;
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
        private AutoRunHelper autoRunHelper = new AutoRunHelper();

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
            IsDevMode = SettingService.Instance.GetOrDefault(Setting.IsDevMode, false);
            this.Log("initialized");
        }

        #region propdp
        public string VersionString
        {
            get => (string)GetValue(VersionStringProperty);
            set => SetValue(VersionStringProperty, value);
        }
        public static readonly DependencyProperty VersionStringProperty =
            DependencyProperty.Register("VersionString", typeof(string), typeof(SettingsPage), new PropertyMetadata(""));

        public ApplicationTheme CurrentTheme
        {
            get => (ApplicationTheme)GetValue(CurrentThemeProperty);
            set => SetValue(CurrentThemeProperty, value);
        }
        public static readonly DependencyProperty CurrentThemeProperty =
            DependencyProperty.Register("CurrentTheme", typeof(ApplicationTheme), typeof(SettingsPage), new PropertyMetadata(null));

        public bool IsDevMode
        {
            get => (bool)GetValue(IsDevModeProperty);
            set
            {
                SettingService.Instance[Setting.IsDevMode] = value;
                SetValue(IsDevModeProperty, value);
            }
        }
        public static readonly DependencyProperty IsDevModeProperty =
            DependencyProperty.Register("IsDevMode", typeof(bool), typeof(DailyPage), new PropertyMetadata(SettingService.Instance.GetOrDefault(Setting.IsDevMode, false)));
        #endregion

        public AutoRunHelper AutoRunHelper { get => autoRunHelper; set => autoRunHelper = value; }

        private async void UpdateRequestedAsync(object sender, RoutedEventArgs e)
        {
            UpdateState u = await UpdateService.Instance.CheckUpdateStateAsync();

            switch (u)
            {
                case UpdateState.NeedUpdate:
                    UpdateService.Instance.DownloadAndInstallPackage();
                    await new UpdateDialog().ShowAsync();
                    break;
                case UpdateState.IsNewestRelease:
                    ((Button)sender).Content = "已是最新版";
                    ((Button)sender).IsEnabled = false;
                    break;
                case UpdateState.IsInsiderVersion:
                    ((Button)sender).Content = "内部测试版";
                    ((Button)sender).IsEnabled = false;
                    break;
                case UpdateState.NotAvailable:
                    ((Button)sender).Content = "获取更新失败";
                    ((Button)sender).IsEnabled = false;
                    break;
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
            await CookieManager.AddCookieAsync();
        }
    }
}
