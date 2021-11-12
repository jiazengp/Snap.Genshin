using DGP.Genshin.Services.Launching;
using DGP.Genshin.Services.Settings;
using Microsoft.Win32;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DGP.Genshin.Controls.TitleBarButtons
{
    /// <summary>
    /// LaunchTitleBarButton.xaml 的交互逻辑
    /// </summary>
    public partial class LaunchTitleBarButton : TitleBarButton
    {
        public LaunchService? Launcher { get; set; }

        public LaunchTitleBarButton()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void LaunchButtonClick(object sender, RoutedEventArgs e)
        {
            Launcher?.Launch(Launcher.CurrentScheme, async ex =>
            {
                await new ContentDialog()
                {
                    Title = "启动失败",
                    Content = ex.Message,
                    PrimaryButtonText = "确定",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            });
        }

        private void LaunchTitleBarButtonClick(object sender, RoutedEventArgs e)
        {
            string? launcherPath = SettingService.Instance.GetOrDefault<string?>(Setting.LauncherPath, null);
            if (!File.Exists(launcherPath) || Path.GetFileNameWithoutExtension(launcherPath) != "launcher")
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = "启动器|launcher.exe",
                    Title = "选择启动器文件",
                    CheckPathExists = true,
                    FileName = "launcher.exe"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    launcherPath = openFileDialog.FileName;
                    SettingService.Instance[Setting.LauncherPath] = launcherPath;
                }
            }
            if (launcherPath is not null)
            {
                Launcher ??= LaunchService.Instance;
                //acquire list here
                Launcher.MatchAccount();
                if (sender.ShowAttachedFlyout<Grid>(Launcher))
                {
                    
                }
            }
        }

        private void Flyout_Closed(object sender, object e)
        {
            Launcher?.SaveAllAccounts();
        }
    }
}
