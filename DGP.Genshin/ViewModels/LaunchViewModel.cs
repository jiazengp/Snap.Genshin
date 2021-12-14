using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Data.Json;
using DGP.Genshin.Controls.TitleBarButtons;
using DGP.Genshin.DataModel.Launching;
using DGP.Genshin.Services.Abstratcions;
using DGP.Genshin.Services.Launching;
using DGP.Genshin.Services.Settings;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DGP.Genshin.ViewModels
{
    [ViewModel(ViewModelType.Transient)]
    public class LaunchViewModel : ObservableObject
    {
        private const string AccountsFile = "accounts.json";
        private readonly ILaunchService launchService;
        private readonly ISettingService settingService;

        /// <summary>
        /// 已知的启动方案
        /// </summary>
        private List<LaunchScheme> knownSchemes = new()
        {
            new LaunchScheme(name: "官服 | 天空岛", channel: "1", cps: "pcadbdpz", subChannel: "1"),
            new LaunchScheme(name: "B 服 | 世界树", channel: "14", cps: "bilibili", subChannel: "0")
        };

        #region Observable
        private LaunchScheme? currentScheme;
        private bool isBorderless;
        private bool isFullScreen;
        private ObservableCollection<GenshinAccount> accounts = new();
        private GenshinAccount? selectedAccount;

        private IAsyncRelayCommand<TitleBarButton> initializeCommand;
        private IRelayCommand<string> launchCommand;
        private IRelayCommand unInitializeCommand;
        private IAsyncRelayCommand deleteAccountCommand;

        /// <summary>
        /// 已知的启动方案
        /// </summary>
        public List<LaunchScheme> KnownSchemes
        {
            get => knownSchemes;
            set => SetProperty(ref knownSchemes, value);
        }
        /// <summary>
        /// 当前启动方案
        /// </summary>
        public LaunchScheme? CurrentScheme
        {
            get => currentScheme;
            set
            {
                SetProperty(ref currentScheme, value);
                launchService.SaveLaunchScheme(value);
            }
        }
        /// <summary>
        /// 是否全屏
        /// </summary>
        public bool IsFullScreen
        {
            get => isFullScreen;
            set
            {
                SetProperty(ref isFullScreen, value);
                settingService[Setting.IsFullScreen] = value;
            }
        }
        /// <summary>
        /// 是否无边框窗口
        /// </summary>
        public bool IsBorderless
        {
            get => isBorderless;
            set
            {
                SetProperty(ref isBorderless, value);
                settingService[Setting.IsBorderless] = value;
            }
        }
        public ObservableCollection<GenshinAccount> Accounts
        {
            get => accounts;
            set => SetProperty(ref accounts, value);
        }
        public GenshinAccount? SelectedAccount
        {
            get => selectedAccount;
            set
            {
                SetProperty(ref selectedAccount, value);
                GenshinRegistry.Set(value);
            }
        }
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

        public LaunchViewModel(ILaunchService launchService, ISettingService settingService)
        {
            this.launchService = launchService;
            this.settingService = settingService;
            accounts = Json.FromFile<ObservableCollection<GenshinAccount>>(AccountsFile) ?? new();
            selectedAccount = accounts.FirstOrDefault();
            //prepare basic launch
            isBorderless = settingService.GetOrDefault(Setting.IsBorderless, false);
            OnPropertyChanged(nameof(IsBorderless));

            InitializeCommand = new AsyncRelayCommand<TitleBarButton>(InitializeAsync);
            LaunchCommand = new RelayCommand<string>(LaunchByOption);
            UnInitializeCommand = new RelayCommand(SaveAllAccounts);
            DeleteAccountCommand = new AsyncRelayCommand(DeleteAccountAsync);
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

        public object _savingFile = new();
        public void SaveAllAccounts()
        {
            lock (_savingFile)
            {
                Json.ToFile(AccountsFile, Accounts);
            }
        }

        private void LaunchByOption(string? option)
        {
            //this.HideAttachedFlyout();
            switch (option)
            {
                case "Launcher":
                    {
                        launchService.OpenOfficialLauncher(async ex =>
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
                        launchService.Launch(CurrentScheme, async ex =>
                        {
                            await new ContentDialog()
                            {
                                Title = "启动失败",
                                Content = ex.Message,
                                PrimaryButtonText = "确定",
                                DefaultButton = ContentDialogButton.Primary
                            }.ShowAsync();
                        }, IsBorderless, IsFullScreen);
                        break;
                    }
            }
        }

        private async Task InitializeAsync(TitleBarButton? t)
        {
            string? launcherPath = settingService.GetOrDefault<string?>(Setting.LauncherPath, null);
            launcherPath = launchService.SelectLaunchDirectory(launcherPath);
            if (launcherPath is not null)
            {
                MatchAccount();
                CurrentScheme = KnownSchemes.First(item => item.Channel == launchService.GameConfig["General"]["channel"]);
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
    }
}
