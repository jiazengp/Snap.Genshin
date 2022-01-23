using DGP.Genshin.Controls.Launching;
using DGP.Genshin.Controls.TitleBarButtons;
using DGP.Genshin.DataModels.Launching;
using DGP.Genshin.Helpers;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.Input;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace DGP.Genshin.ViewModels.TitleBarButtons
{
    [ViewModel(InjectAs.Transient)]
    public class LaunchViewModel : ObservableObject2
    {
        private readonly ILaunchService launchService;
        private readonly ISettingService settingService;

        private TitleBarButton? View;

        #region Observables
        private List<LaunchScheme> knownSchemes = new()
        {
            new LaunchScheme(name: "天空岛 | 官方服", channel: "1", cps: "pcadbdpz", subChannel: "1"),
            new LaunchScheme(name: "世界树 | Bili服", channel: "14", cps: "bilibili", subChannel: "0")
        };

        private LaunchScheme? currentScheme;
        private bool isBorderless;
        private bool isFullScreen;
        private ObservableCollection<GenshinAccount> accounts = new();
        private GenshinAccount? selectedAccount;
        private bool unlockFPS;
        private double targetFPS;

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
            set => SetPropertyAndCallbackOnCompletion(ref currentScheme, value, v => launchService.SaveLaunchScheme(v));
        }
        /// <summary>
        /// 是否全屏
        /// </summary>
        public bool IsFullScreen
        {
            get => isFullScreen;
            set => SetPropertyAndCallbackOnCompletion(ref isFullScreen, value, v => settingService[Setting.IsFullScreen] = value);
        }
        /// <summary>
        /// 是否无边框窗口
        /// </summary>
        public bool IsBorderless
        {
            get => isBorderless;
            set => SetPropertyAndCallbackOnCompletion(ref isBorderless, value, v => settingService[Setting.IsBorderless] = value);
        }
        /// <summary>
        /// 是否解锁FPS上限
        /// </summary>
        public bool UnlockFPS
        {
            get => unlockFPS;
            set => SetPropertyAndCallbackOnCompletion(ref unlockFPS, value, v => settingService[Setting.UnlockFPS] = v);
        }
        /// <summary>
        /// 目标帧率
        /// </summary>
        public double TargetFPS
        {
            get => targetFPS;
            set => SetPropertyAndCallbackOnCompletion(ref targetFPS, value, v => settingService[Setting.TargetFPS] = v);
        }

        private bool? isElevated;
        public bool IsElevated
        {
            get
            {
                isElevated ??= App.IsElevated;
                return isElevated.Value;
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
            set => SetPropertyAndCallbackOnCompletion(ref selectedAccount, value, v => launchService.SetToRegistry(v));
        }

        public ICommand OpenUICommand { get; }
        public ICommand CloseUICommand { get; }
        public ICommand LaunchCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        #endregion

        public LaunchViewModel(ILaunchService launchService, ISettingService settingService)
        {
            this.launchService = launchService;
            this.settingService = settingService;

            Accounts = launchService.LoadAllAccount();
            SelectedAccount = Accounts.FirstOrDefault();

            IsBorderless = settingService.GetOrDefault(Setting.IsBorderless, false);
            IsFullScreen = settingService.GetOrDefault(Setting.IsFullScreen, false);
            UnlockFPS = settingService.GetOrDefault(Setting.UnlockFPS, false);
            TargetFPS = settingService.GetOrDefault(Setting.TargetFPS, 60.0);

            OpenUICommand = new AsyncRelayCommand<TitleBarButton>(OpenUIAsync);
            LaunchCommand = new AsyncRelayCommand<string>(LaunchByOption);
            CloseUICommand = new RelayCommand(SaveAllAccounts);
            DeleteAccountCommand = new RelayCommand(DeleteAccount);
        }

        private async Task OpenUIAsync(TitleBarButton? t)
        {
            string? launcherPath = settingService.GetOrDefault<string?>(Setting.LauncherPath, null);
            launcherPath = launchService.SelectLaunchDirectoryIfNull(launcherPath);
            bool result = false;
            if (launcherPath is not null && launchService.TryLoadIniData(launcherPath))
            {
                await MatchAccountAsync();
                CurrentScheme = KnownSchemes.First(item => item.Channel == launchService.GameConfig["General"]["channel"]);
                t?.ShowAttachedFlyout<Grid>(this);
                View = t;
                result = true;
            }
            else
            {
                await App.Current.Dispatcher.InvokeAsync(new ContentDialog()
                {
                    Title = "无法使用此功能",
                    Content = "我们需要启动器的路径才能启动游戏。\n如果你已经选择了正确的文件夹但仍看到此提示，\n请联系开发者。",
                    PrimaryButtonText = "确定"
                }.ShowAsync).Task.Unwrap();
            }
            new Event(t?.GetType(), result).TrackAs(Event.OpenTitle);
        }
        private async Task LaunchByOption(string? option)
        {
            View?.HideAttachedFlyout();
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
                        LaunchOption? launchOption = new()
                        {
                            IsBorderless = IsBorderless,
                            IsFullScreen = IsFullScreen,
                            UnlockFPS = IsElevated && UnlockFPS,
                            TargetFPS = (int)TargetFPS
                        };

                        await launchService.LaunchAsync(CurrentScheme, async ex =>
                        {
                            await new ContentDialog()
                            {
                                Title = "启动游戏失败",
                                Content = ex.Message,
                                PrimaryButtonText = "确定",
                                DefaultButton = ContentDialogButton.Primary
                            }.ShowAsync();
                        }, launchOption);
                        break;
                    }
            }
        }
        private void SaveAllAccounts()
        {
            launchService.SaveAllAccounts(Accounts);
        }
        private void DeleteAccount()
        {
            if (SelectedAccount is not null)
            {
                View?.HideAttachedFlyout();
                Accounts.Remove(SelectedAccount);
                SelectedAccount = Accounts.LastOrDefault();
            }
        }

        /// <summary>
        /// 从注册表获取当前的账户信息
        /// </summary>
        private async Task MatchAccountAsync()
        {
            //注册表内有账号信息
            if (launchService.GetFromRegistry() is GenshinAccount currentRegistryAccount)
            {
                GenshinAccount? matched = Accounts.FirstOrDefault(
                    a => a.GeneralData == currentRegistryAccount.GeneralData
                    && a.MihoyoSDK == currentRegistryAccount.MihoyoSDK);
                //账号列表内无匹配项
                if (matched is null)
                {
                    //命名
                    currentRegistryAccount.Name = await new NameDialog { TargetAccount = currentRegistryAccount }.GetInputAsync();
                    Accounts.Add(currentRegistryAccount);
                    selectedAccount = currentRegistryAccount;
                }
                else
                {
                    selectedAccount = matched;
                }
                //prevent registry set
                OnPropertyChanged(nameof(SelectedAccount));
            }
        }
    }
}
