using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Data.Json;
using DGP.Genshin.Controls.TitleBarButtons;
using DGP.Genshin.Services.Abstratcions;
using DGP.Genshin.Services.Settings;
using IniParser;
using IniParser.Model;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.Services.Launching
{
    /// <summary>
    /// 游戏启动服务
    /// </summary>
    [ViewModel(ViewModelType.Transient)]
    public class LaunchViewModel : ObservableObject
    {
        private const string AccountsFile = "accounts.json";
        private List<LaunchScheme> knownSchemes = new()
        {
            new LaunchScheme(name: "官服 | 天空岛", channel: "1", cps: "pcadbdpz", subChannel: "1"),
            new LaunchScheme(name: "B 服 | 世界树", channel: "14", cps: "bilibili", subChannel: "0")
        };

        private ISettingService settingService;

        #region Observable
        private LaunchScheme currentScheme;
        private bool isBorderless;
        private bool isFullScreen;
        private ObservableCollection<GenshinAccount> accounts;
        private GenshinAccount? selectedAccount;

        private IAsyncRelayCommand<TitleBarButton> initializeCommand;
        private IRelayCommand<string> launchCommand;
        private IRelayCommand unInitializeCommand;
        private IAsyncRelayCommand deleteAccountCommand;

        /// <summary>
        /// 已知的启动方案
        /// </summary>
        public List<LaunchScheme> KnownSchemes { get => knownSchemes; set => SetProperty(ref knownSchemes, value); }

        /// <summary>
        /// 当前启动方案
        /// </summary>
        public LaunchScheme CurrentScheme { get => currentScheme; set { SetProperty(ref currentScheme, value); OnCurrentSchemeChanged(); } }

        /// <summary>
        /// 在此处保存更改后的配置
        /// 以便游戏启动后能读取到
        /// </summary>
        private void OnCurrentSchemeChanged()
        {
            gameConfig["General"]["channel"] = currentScheme.Channel;
            gameConfig["General"]["cps"] = currentScheme.CPS;
            gameConfig["General"]["sub_channel"] = currentScheme.SubChannel;

            string unescapedGameFolder = Regex.Unescape(launcherConfig["launcher"]["game_install_path"].Replace("x", "u"));
            //compat with https://github.com/DawnFz/GenShin-LauncherDIY
            new FileIniDataParser().WriteFile($@"{unescapedGameFolder}\config.ini", gameConfig, new UTF8Encoding(false));
        }

        public bool IsFullScreen { get => isFullScreen; set { SetProperty(ref isFullScreen, value); settingService[Setting.IsFullScreen] = value; } }

        /// <summary>
        /// 是否启用无边框窗口模式
        /// </summary>
        public bool IsBorderless { get => isBorderless; set { SetProperty(ref isBorderless, value); settingService[Setting.IsBorderless] = value; } }

        public ObservableCollection<GenshinAccount> Accounts { get => accounts; set => SetProperty(ref accounts, value); }

        public GenshinAccount? SelectedAccount { get => selectedAccount; set { SetProperty(ref selectedAccount, value); GenshinRegistry.Set(value); } }

        public IAsyncRelayCommand<TitleBarButton> InitializeCommand
        {
            get => initializeCommand;
            [MemberNotNull(nameof(initializeCommand))]
            set => SetProperty(ref initializeCommand, value);
        }
        public IRelayCommand UnInitializeCommand
        {
            get => unInitializeCommand;
            [MemberNotNull(nameof(unInitializeCommand))]
            set => SetProperty(ref unInitializeCommand, value);
        }
        public IRelayCommand<string> LaunchCommand
        {
            get => launchCommand;
            [MemberNotNull(nameof(launchCommand))]
            set => SetProperty(ref launchCommand, value);
        }
        public IAsyncRelayCommand DeleteAccountCommand
        {
            get => deleteAccountCommand;
            [MemberNotNull(nameof(deleteAccountCommand))]
            set => SetProperty(ref deleteAccountCommand, value);
        }
        #endregion

        #region Basic Launch
        private readonly IniData launcherConfig;
        private readonly IniData gameConfig;

        /// <summary>
        /// 启动游戏
        /// </summary>
        /// <param name="scheme">配置方案</param>
        /// <param name="failAction">启动失败回调</param>
        public void Launch(LaunchScheme scheme, Action<Exception> failAction)
        {
            string? launcherPath = settingService.GetOrDefault<string?>(Setting.LauncherPath, null);
            if (launcherPath is not null)
            {
                string unescapedGameFolder = GetUnescapedGameFolder();
                string gamePath = $@"{unescapedGameFolder}/{launcherConfig["launcher"]["game_start_name"]}";
                gamePath = Regex.Unescape(gamePath);

                try
                {
                    ProcessStartInfo info = new()
                    {
                        FileName = gamePath,
                        Verb = "runas",
                        UseShellExecute = true,
                        Arguments = $"{(IsBorderless ? "-popupwindow " : "")}-screen-fullscreen {(IsFullScreen ? 1 : 0)}"
                    };
                    Process? p = Process.Start(info);
                }
                catch (Exception ex)
                {
                    failAction?.Invoke(ex);
                }
            }
        }

        public void OpenOfficialLauncher(Action<Exception>? failAction)
        {
            string? launcherPath = settingService.GetOrDefault<string?>(Setting.LauncherPath, null);
            try
            {
                ProcessStartInfo info = new()
                {
                    FileName = launcherPath,
                    Verb = "runas",
                    UseShellExecute = true,
                };
                Process? p = Process.Start(info);
            }
            catch (Exception ex)
            {
                failAction?.Invoke(ex);
            }
        }

        /// <summary>
        /// 还原转义后的原游戏目录
        /// 目录符号应为/
        /// 因为配置中的游戏目录若包含中文会转义为 \xaaaa 形态
        /// </summary>
        /// <returns></returns>
        private string GetUnescapedGameFolder()
        {
            string gameInstallPath = launcherConfig["launcher"]["game_install_path"];
            return Regex.Unescape(gameInstallPath.Replace(@"\x", @"\u"));
        }

        /// <summary>
        /// 等待原神进程退出，若找不到原神进程会直接返回
        /// </summary>
        /// <returns></returns>
        public Task WaitGenshinImpactExitAsync()
        {
            Process[] procs = Process.GetProcessesByName("YuanShen");
            if (procs.Any())
            {
                return Task.WhenAll(procs.Select(p => p.WaitForExitAsync()));
            }
            return Task.CompletedTask;
        }
        #endregion

        #region Account Switch
        /// <summary>
        /// 从注册表获取当前的账户信息
        /// </summary>
        public async void MatchAccount()
        {
            //注册表内有账号信息
            if (GenshinRegistry.Get() is GenshinAccount current)
            {
                GenshinAccount? matched = Accounts.FirstOrDefault(a => a.GeneralData == current.GeneralData && a.MihoyoSDK == current.MihoyoSDK);
                //账号列表内无匹配项
                if (matched is null)
                {
                    //命名
                    current.Name = await new NameDialog { TargetAccount = current }.GetInputAsync();
                    Accounts.Add(current);
                    selectedAccount = current;
                }
                else
                {
                    selectedAccount = matched;
                }
                OnPropertyChanged(nameof(SelectedAccount));
            }
        }
        public object _savingFile = new();


        public void SaveAllAccounts()
        {
            lock (_savingFile)
            {
                Json.ToFile(AccountsFile, Accounts);
            }
        }
        #endregion

        private string? SelectLaunchDirectory(string? launcherPath)
        {
            if (!File.Exists(launcherPath) || Path.GetFileNameWithoutExtension(launcherPath) != "launcher")
            {
                OpenFileDialog openFileDialog = new()
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
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
                        settingService[Setting.LauncherPath] = launcherPath;
                    }
                }
            }

            return launcherPath;
        }

        private LaunchViewModel(ISettingService settingService)
        {
            this.settingService = settingService;
            InitializeCommand = new AsyncRelayCommand<TitleBarButton>(InitializeAsync);
            LaunchCommand = new RelayCommand<string>(LaunchByOption);
            UnInitializeCommand = new RelayCommand(SaveAllAccounts);
            DeleteAccountCommand = new AsyncRelayCommand(async () =>
            {
                await DeleteAccountAsync();
            });
            //prepare accounts
            if (!File.Exists(AccountsFile))
            {
                File.Create(AccountsFile).Dispose();
            }
            accounts = Json.FromFile<ObservableCollection<GenshinAccount>>(AccountsFile) ?? new();
            selectedAccount = accounts.FirstOrDefault();
            //prepare basic launch
            isBorderless = settingService.GetOrDefault(Setting.IsBorderless, false);
            OnPropertyChanged(nameof(IsBorderless));

            string? launcherPath = settingService.GetOrDefault<string?>(Setting.LauncherPath, null);
            string configPath = $@"{Path.GetDirectoryName(launcherPath)}\config.ini";

            FileIniDataParser launcherParser = new();
            launcherParser.Parser.Configuration.AssigmentSpacer = "";
            launcherConfig = launcherParser.ReadFile(configPath);

            string unescapedGameFolder = GetUnescapedGameFolder();

            FileIniDataParser gameParser = new();
            gameParser.Parser.Configuration.AssigmentSpacer = "";
            gameConfig = gameParser.ReadFile($@"{unescapedGameFolder}\config.ini");

            currentScheme = KnownSchemes.First(item => item.Channel == gameConfig["General"]["channel"]);
            OnPropertyChanged(nameof(CurrentScheme));
        }

        private async Task DeleteAccountAsync()
        {
            if (SelectedAccount is not null)
            {
                if (Accounts.Count <= 1)
                {
                    //this.HideAttachedFlyout();
                    await App.Current.Dispatcher.InvokeAsync(new ContentDialog()
                    {
                        Title = "删除账户失败",
                        Content = "我们需要至少一组信息才能正常启动游戏。",
                        PrimaryButtonText = "确定"
                    }.ShowAsync).Task.Unwrap();
                    return;
                }
                Accounts.Remove(SelectedAccount);
                SelectedAccount = Accounts.Last();
            }
        }

        private void LaunchByOption(string? s)
        {
            //this.HideAttachedFlyout();
            switch (s)
            {
                case "Launcher":
                    {
                        OpenOfficialLauncher(async ex =>
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
                        Launch(CurrentScheme, async ex =>
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

        private async Task InitializeAsync(TitleBarButton? t)
        {
            string? launcherPath = settingService.GetOrDefault<string?>(Setting.LauncherPath, null);
            launcherPath = SelectLaunchDirectory(launcherPath);
            if (launcherPath is not null)
            {
                MatchAccount();
                t?.ShowAttachedFlyout<Grid>(this);
            }
            else
            {
                await App.Current.Dispatcher.InvokeAsync(new ContentDialog()
                {
                    Title = "打开面板失败",
                    Content = "我们需要启动器的路径才能正常启动游戏。",
                    PrimaryButtonText = "确定"
                }.ShowAsync).Task.Unwrap();
            }
        }
    }
}
