using DGP.Genshin.Control.Launching;
using DGP.Genshin.Core.Notification;
using DGP.Genshin.DataModel.Launching;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Helper.Extension;
using DGP.Genshin.Message;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstraction.Launching;
using DGP.Genshin.Service.Abstraction.Setting;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.VisualStudio.Threading;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using Snap.Data.Primitive;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    [ViewModel(InjectAs.Transient)]
    internal class LaunchViewModel : ObservableObject2
    {
        private readonly ILaunchService launchService;
        private readonly IMessenger messenger;

        #region Observables
        private List<LaunchScheme> knownSchemes = new()
        {
            new LaunchScheme(name: "官方服 | 天空岛", channel: "1", cps: "pcadbdpz", subChannel: "1"),
            new LaunchScheme(name: "渠道服 | 世界树", channel: "14", cps: "bilibili", subChannel: "0"),
            new LaunchScheme(name: "国际服 | 需要插件", channel: "1", cps: "mihoyo", subChannel: "0")
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
            get => this.knownSchemes;

            set => this.SetProperty(ref this.knownSchemes, value);
        }
        /// <summary>
        /// 当前启动方案
        /// </summary>
        public LaunchScheme? CurrentScheme
        {
            get => this.currentScheme;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.currentScheme, value, v => this.launchService.SaveLaunchScheme(v));
        }
        /// <summary>
        /// 是否全屏
        /// </summary>
        public bool IsFullScreen
        {
            get => this.isFullScreen;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.isFullScreen, value, Setting2.IsFullScreen.Set);
        }
        /// <summary>
        /// 是否无边框窗口
        /// </summary>
        public bool IsBorderless
        {
            get => this.isBorderless;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.isBorderless, value, Setting2.IsBorderless.Set);
        }
        /// <summary>
        /// 是否解锁FPS上限
        /// </summary>
        public bool UnlockFPS
        {
            get => this.unlockFPS;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.unlockFPS, value, Setting2.UnlockFPS.Set);
        }
        /// <summary>
        /// 目标帧率
        /// </summary>
        public double TargetFPS
        {
            get => this.targetFPS;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.targetFPS, value, this.OnTargetFPSChanged);
        }
        [PropertyChangedCallback]
        private void OnTargetFPSChanged(double value)
        {
            Setting2.TargetFPS.Set(value);
            this.launchService.SetTargetFPSDynamically((int)value);
        }
        public long ScreenWidth
        {
            get => this.screenWidth;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.screenWidth, value, Setting2.ScreenWidth.Set);
        }
        public long ScreenHeight
        {
            get => this.screenHeight;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.screenHeight, value, Setting2.ScreenHeight.Set);
        }
        public bool IsElevated
        {
            get
            {
                this.isElevated ??= App.IsElevated;
                return this.isElevated.Value;
            }
        }
        public ObservableCollection<GenshinAccount> Accounts
        {
            get => this.accounts;

            set => this.SetProperty(ref this.accounts, value);
        }
        public GenshinAccount? SelectedAccount
        {
            get => this.selectedAccount;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.selectedAccount, value, v => this.launchService.SetToRegistry(v));
        }

        public WorkWatcher GameWatcher { get; }

        public ICommand OpenUICommand { get; }
        public ICommand LaunchCommand { get; }
        public ICommand MatchAccountCommand { get; }
        public ICommand RenameAccountCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand ReselectLauncherPathCommand { get; }
        #endregion

        public LaunchViewModel(IMessenger messenger, IAsyncRelayCommandFactory asyncRelayCommandFactory)
        {
            this.launchService = App.Current.SwitchableImplementationManager.CurrentLaunchService!.Factory.Value;
            this.GameWatcher = this.launchService.GameWatcher;

            this.messenger = messenger;

            this.Accounts = this.launchService.LoadAllAccount();

            this.IsBorderless = Setting2.IsBorderless;
            this.IsFullScreen = Setting2.IsFullScreen;
            this.UnlockFPS = Setting2.UnlockFPS;
            this.TargetFPS = Setting2.TargetFPS;
            this.ScreenWidth = Setting2.ScreenWidth;
            this.ScreenHeight = Setting2.ScreenHeight;

            this.OpenUICommand = asyncRelayCommandFactory.Create(this.OpenUIAsync);
            this.LaunchCommand = asyncRelayCommandFactory.Create<string>(this.LaunchByOptionAsync);
            this.MatchAccountCommand = asyncRelayCommandFactory.Create(() => this.MatchAccountAsync(true));
            this.RenameAccountCommand = asyncRelayCommandFactory.Create(this.RenameAccountAsync);
            this.DeleteAccountCommand = new RelayCommand(this.DeleteAccount);
            this.ReselectLauncherPathCommand = asyncRelayCommandFactory.Create(this.ReselectLauncherPathAsync);
        }

        private async Task OpenUIAsync()
        {
            string? launcherPath = Setting2.LauncherPath;
            launcherPath = this.launchService.SelectLaunchDirectoryIfIncorrect(launcherPath);
            if (launcherPath is not null && this.launchService.TryLoadIniData(launcherPath))
            {
                await this.MatchAccountAsync();
                this.CurrentScheme = this.KnownSchemes
                    .First(item => item.CPS == this.launchService.GameConfig["General"]["cps"]);
            }
            else
            {
                Setting2.LauncherPath.Set(null);

                await this.ExecuteOnUIAsync(new ContentDialog()
                {
                    Title = "请尝试重新选择启动器路径",
                    Content = "可能是启动器路径设置错误\n或者读取游戏配置文件失败",
                    PrimaryButtonText = "确定"
                }.ShowAsync);
                this.messenger.Send(new NavigateRequestMessage(typeof(HomePage), true));
            }
        }
        private async Task LaunchByOptionAsync(string? option)
        {
            switch (option)
            {
                case "Launcher":
                    {
                        this.launchService.OpenOfficialLauncher(ex =>
                        this.HandleLaunchFailureAsync("打开启动器失败", ex).Forget());
                        break;
                    }
                case "Game":
                    {
                        await this.launchService.LaunchAsync(LaunchOption.FromCurrentSettings(), ex =>
                        this.HandleLaunchFailureAsync("启动游戏失败", ex).Forget());
                        break;
                    }
            }
        }
        private void SaveAllAccounts()
        {
            this.launchService.SaveAllAccounts(this.Accounts);
        }
        private async Task RenameAccountAsync()
        {
            if (this.SelectedAccount is not null)
            {
                this.SelectedAccount.Name = await new NameDialog { TargetAccount = SelectedAccount }.GetInputAsync();
                this.SaveAllAccounts();
            }
        }
        private void DeleteAccount()
        {
            if (this.SelectedAccount is not null)
            {
                this.Accounts.Remove(this.SelectedAccount);
                this.SelectedAccount = this.Accounts.LastOrDefault();
                this.SaveAllAccounts();
            }
        }

        /// <summary>
        /// 从注册表获取当前的账户信息
        /// </summary>
        private async Task MatchAccountAsync(bool allowNewAccount = false)
        {
            //注册表内有账号信息
            if (this.launchService.GetFromRegistry() is GenshinAccount currentRegistryAccount)
            {
                GenshinAccount? matched = this.Accounts.FirstOrDefault(a => a.MihoyoSDK == currentRegistryAccount.MihoyoSDK);
                //账号列表内存在匹配项
                if (matched is not null)
                {
                    this.selectedAccount = matched;
                }
                else
                {
                    if (allowNewAccount)
                    {
                        //命名
                        currentRegistryAccount.Name = await new NameDialog { TargetAccount = currentRegistryAccount }.GetInputAsync();
                        this.Accounts.Add(currentRegistryAccount);
                        this.selectedAccount = currentRegistryAccount;
                    }
                }
                //prevent registry set
                this.OnPropertyChanged(nameof(this.SelectedAccount));
                this.SaveAllAccounts();
            }
            else
            {
                this.SelectedAccount = this.Accounts.FirstOrDefault();
                new ToastContentBuilder()
                .AddText("从注册表获取账号信息失败")
                .SafeShow();
            }
        }
        private async Task ReselectLauncherPathAsync()
        {
            Setting2.LauncherPath.Set(null);
            string? launcherPath = Setting2.LauncherPath;
            launcherPath = this.launchService.SelectLaunchDirectoryIfIncorrect(launcherPath);
            if (launcherPath is not null && this.launchService.TryLoadIniData(launcherPath))
            {
                await this.MatchAccountAsync();
                this.CurrentScheme = this.KnownSchemes
                    .First(item => item.Channel == this.launchService.GameConfig["General"]["channel"]);
            }
            else
            {
                Setting2.LauncherPath.Set(null);
                await this.ExecuteOnUIAsync(new ContentDialog()
                {
                    Title = "请尝试重新选择启动器路径",
                    Content = "可能是启动器路径设置错误\n或者读取游戏配置文件失败",
                    PrimaryButtonText = "确定"
                }.ShowAsync);
                this.messenger.Send(new NavigateRequestMessage(typeof(HomePage), true));
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
