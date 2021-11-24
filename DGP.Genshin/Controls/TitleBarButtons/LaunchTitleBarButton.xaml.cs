using DGP.Genshin.Services.Launching;
using DGP.Genshin.Services.Settings;
using Microsoft.Win32;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            this.HideAttachedFlyout();
            switch (((FrameworkElement)sender).Tag)
            {
                case "Launcher":
                    {
                        Launcher?.OpenOfficialLauncher(async ex =>
                        {
                            await new ContentDialog()
                            {
                                Title = "打开启动器失败",
                                Content = ex.Message,
                                PrimaryButtonText = "确定",
                                DefaultButton = ContentDialogButton.Primary
                            }.ShowAsync();
                        });
                        break;
                    }
                case "Game":
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
                        break;
                    }
            }
        }

        private void LaunchTitleBarButtonClick(object sender, RoutedEventArgs e)
        {
            string? launcherPath = SettingService.Instance.GetOrDefault<string?>(Setting.LauncherPath, null);
            launcherPath = SelectLaunchDirectory(launcherPath);
            if (launcherPath is not null)
            {
                Launcher ??= LaunchService.Instance;
                //acquire list here
                Launcher.MatchAccount();
                if (sender.ShowAttachedFlyout<Grid>(Launcher))
                {

                }
            }
            else
            {
                //TODO remind user to select correct exe.
            }
        }

        private static string? SelectLaunchDirectory(string? launcherPath)
        {
            if (!File.Exists(launcherPath) || Path.GetFileNameWithoutExtension(launcherPath) != "launcher")
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = "启动器|launcher.exe|快捷方式|*.lnk",
                    Title = "选择启动器文件",
                    CheckPathExists = true,
                    DereferenceLinks = true,
                    FileName = "launcher.exe"
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    string fileName = openFileDialog.FileName;
                    if (Path.GetFileNameWithoutExtension(fileName) == "launcher")
                    {
                        launcherPath = openFileDialog.FileName;
                        SettingService.Instance[Setting.LauncherPath] = launcherPath;
                    }
                }
            }

            return launcherPath;
        }

        private void Flyout_Closed(object sender, object e)
        {
            Launcher?.SaveAllAccounts();
        }

        private async void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            if(Launcher is not null && Launcher.SelectedAccount is not null)
            {
                if (Launcher.Accounts.Count <= 1)
                {
                    this.HideAttachedFlyout();
                    await App.Current.Dispatcher.InvokeAsync(new ContentDialog()
                    {
                        Title = "删除账户失败",
                        Content = "我们需要至少一组信息才能使程序正常启动游戏。",
                        PrimaryButtonText = "确定"
                    }.ShowAsync).Task.Unwrap();
                    return;
                }
                Launcher.Accounts.Remove(Launcher.SelectedAccount);
                Launcher.SelectedAccount = Launcher.Accounts.Last();
            }
        }
    }
}
