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
        public SettingsPage()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //version
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            this.VersionString = $"DGP.Genshin - version {v.Major}.{v.Minor}.{v.Build} Build {v.Revision}";
            //theme
            Func<object, ApplicationTheme?> converter = n => n == null ? null : (ApplicationTheme)Enum.Parse(typeof(ApplicationTheme), n.ToString());
            this.ThemeComboBox.SelectedIndex = SettingService.Instance.GetOrDefault(Setting.AppTheme, null, converter) switch
            {
                ApplicationTheme.Light => 0,
                ApplicationTheme.Dark => 1,
                _ => 2,
            };
            this.IsDevMode = SettingService.Instance.GetOrDefault(Setting.IsDevMode, false);
        }

        #region propdp
        public UpdateInfo UpdateInfo
        {
            get => (UpdateInfo)this.GetValue(UpdateInfoProperty);
            set => this.SetValue(UpdateInfoProperty, value);
        }
        public static readonly DependencyProperty UpdateInfoProperty =
            DependencyProperty.Register("UpdateInfo", typeof(UpdateInfo), typeof(SettingsPage), new PropertyMetadata(new UpdateInfo()));

        public string VersionString
        {
            get => (string)this.GetValue(VersionStringProperty);
            set => this.SetValue(VersionStringProperty, value);
        }
        public static readonly DependencyProperty VersionStringProperty =
            DependencyProperty.Register("VersionString", typeof(string), typeof(SettingsPage), new PropertyMetadata(""));

        public ApplicationTheme CurrentTheme
        {
            get => (ApplicationTheme)this.GetValue(CurrentThemeProperty);
            set => this.SetValue(CurrentThemeProperty, value);
        }
        public static readonly DependencyProperty CurrentThemeProperty =
            DependencyProperty.Register("CurrentTheme", typeof(ApplicationTheme), typeof(SettingsPage), new PropertyMetadata(null));

        public bool IsDevMode
        {
            get => (bool)this.GetValue(IsDevModeProperty);
            set
            {
                SettingService.Instance[Setting.IsDevMode] = value;
                this.SetValue(IsDevModeProperty, value);
            }
        }
        public static readonly DependencyProperty IsDevModeProperty =
            DependencyProperty.Register("IsDevMode", typeof(bool), typeof(HomePage), new PropertyMetadata(SettingService.Instance.GetOrDefault(Setting.IsDevMode, false)));
        #endregion

        private async void UpdateRequested(object sender, RoutedEventArgs e)
        {
            UpdateService.Instance.UpdateInfo = this.UpdateInfo;
            UpdateState u = ((Button)sender).Tag.ToString() switch
            {
                "Github" => UpdateService.Instance.CheckUpdateStateViaGithub(),
                _ => UpdateService.Instance.CheckUpdateStateViaGitee(),
            };
            switch (u)
            {
                case UpdateState.NeedUpdate:
                    UpdateService.Instance.DownloadAndInstallPackage();
                    await this.UpdateDialog.ShowAsync();
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
        private void UpdateCancellationRequested(ModernWpf.Controls.ContentDialog sender, ModernWpf.Controls.ContentDialogButtonClickEventArgs args) => UpdateService.Instance.CancelUpdate();
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
        internal static void SetAppTheme()
        {
            static ApplicationTheme? converter(object n) => n == null ? null : (ApplicationTheme)Enum.Parse(typeof(ApplicationTheme), n.ToString());
            ThemeManager.Current.ApplicationTheme = SettingService.Instance.GetOrDefault(Setting.AppTheme, null, converter);
        }
    }
}
