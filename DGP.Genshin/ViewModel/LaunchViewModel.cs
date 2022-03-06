using DGP.Genshin.Control.Launching;
using DGP.Genshin.Core.Notification;
using DGP.Genshin.DataModel.Launching;
using DGP.Genshin.Message;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.VisualStudio.Threading;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Singleton)]
    public class LaunchViewModel : ObservableObject2
    {
        private readonly ILaunchService launchService;
        private readonly IMessenger messenger;

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
        private bool? isElevated;
        private long screenWidth;
        private long screenHeight;

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

            set => SetPropertyAndCallbackOnCompletion(ref isFullScreen, value, Setting2.IsFullScreen.Set);
        }
        /// <summary>
        /// 是否无边框窗口
        /// </summary>
        public bool IsBorderless
        {
            get => isBorderless;

            set => SetPropertyAndCallbackOnCompletion(ref isBorderless, value, Setting2.IsBorderless.Set);
        }
        /// <summary>
        /// 是否解锁FPS上限
        /// </summary>
        public bool UnlockFPS
        {
            get => unlockFPS;

            set => SetPropertyAndCallbackOnCompletion(ref unlockFPS, value, Setting2.UnlockFPS.Set);
        }
        /// <summary>
        /// 目标帧率
        /// </summary>
        public double TargetFPS
        {
            get => targetFPS;

            set => SetPropertyAndCallbackOnCompletion(ref targetFPS, value, OnTargetFPSChanged);
        }
        [PropertyChangedCallback]
        private void OnTargetFPSChanged(double value)
        {
            Setting2.TargetFPS.Set(value);
            launchService.SetTargetFPSDynamically((int)value);
        }
        public long ScreenWidth
        {
            get => screenWidth;

            set => SetPropertyAndCallbackOnCompletion(ref screenWidth, value, Setting2.ScreenWidth.Set);
        }
        public long ScreenHeight
        {
            get => screenHeight;

            set => SetPropertyAndCallbackOnCompletion(ref screenHeight, value, Setting2.ScreenHeight.Set);
        }
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
        public ICommand LaunchCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        #endregion

        public LaunchViewModel(ILaunchService launchService, IMessenger messenger)
        {
            this.launchService = launchService;
            this.messenger = messenger;

            Accounts = launchService.LoadAllAccount();

            IsBorderless = Setting2.IsBorderless.Get();
            IsFullScreen = Setting2.IsFullScreen.Get();
            UnlockFPS = Setting2.UnlockFPS.Get();
            TargetFPS = Setting2.TargetFPS.Get();
            ScreenWidth = Setting2.ScreenWidth.Get();
            ScreenHeight = Setting2.ScreenHeight.Get();

            OpenUICommand = new AsyncRelayCommand(OpenUIAsync);
            LaunchCommand = new AsyncRelayCommand<string>(LaunchByOptionAsync);
            DeleteAccountCommand = new RelayCommand(DeleteAccount);
        }

        private async Task OpenUIAsync()
        {
            string? launcherPath = Setting2.LauncherPath.Get();
            launcherPath = launchService.SelectLaunchDirectoryIfIncorrect(launcherPath);
            if (launcherPath is not null && launchService.TryLoadIniData(launcherPath))
            {
                await MatchAccountAsync();
                CurrentScheme = KnownSchemes
                    .First(item => item.Channel == launchService.GameConfig["General"]["channel"]);
            }
            else
            {
                Setting2.LauncherPath.Set(null);
                await App.Current.Dispatcher.InvokeAsync(new ContentDialog()
                {
                    Title = "无法使用此功能",
                    Content = "可能是启动器路径设置错误\n或者读取游戏配置文件失败\n请尝试重新选择启动器路径",
                    PrimaryButtonText = "确定"
                }.ShowAsync).Task.Unwrap();
                messenger.Send(new NavigateRequestMessage(typeof(HomePage),true));
            }
        }
        private async Task LaunchByOptionAsync(string? option)
        {
            switch (option)
            {
                case "Launcher":
                    {
                        launchService.OpenOfficialLauncher(ex =>
                        HandleLaunchFailureAsync("打开启动器失败", ex).Forget());
                        break;
                    }
                case "Game":
                    {
                        await launchService.LaunchAsync(LaunchOption.FromCurrentSettings(), ex =>
                        HandleLaunchFailureAsync("启动游戏失败", ex).Forget());
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
                Accounts.Remove(SelectedAccount);
                SelectedAccount = Accounts.LastOrDefault();
                SaveAllAccounts();
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
                GenshinAccount? matched = Accounts.FirstOrDefault(a => a.MihoyoSDK == currentRegistryAccount.MihoyoSDK);
                //账号列表内存在匹配项
                if (matched is not null)
                {
                    //账号信息相同但设置不同，优先选择注册表内的设置
                    matched.GeneralData = currentRegistryAccount.GeneralData;
                    selectedAccount = matched;
                }
                else
                {

                    //命名
                    currentRegistryAccount.Name = await new NameDialog { TargetAccount = currentRegistryAccount }.GetInputAsync();
                    Accounts.Add(currentRegistryAccount);
                    selectedAccount = currentRegistryAccount;
                }
                //prevent registry set
                OnPropertyChanged(nameof(SelectedAccount));
                SaveAllAccounts();
            }
            else
            {
                SelectedAccount = Accounts.FirstOrDefault();
                new ToastContentBuilder()
                .AddText("从注册表获取账号信息失败")
                .AddText("已尝试为您切换到第一个账号")
                .SafeShow();
            }
        }
        private async Task HandleLaunchFailureAsync(string title, Exception exception)
        {
            await new ContentDialog()
            {
                Title = title,
                Content = exception.Message,
                PrimaryButtonText = "确定",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
        }
    }
}
