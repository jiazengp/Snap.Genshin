using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DGP.Genshin.Core.Notification;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Message;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstraction.Setting;
using DGP.Genshin.Service.Abstraction.Updating;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using Snap.Data.Primitive;
using Snap.Data.Utility;
using Snap.Win32;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    /// <summary>
    /// 为需要及时响应的设置项提供 <see cref="Observable"/> 模型支持
    /// </summary>
    [ViewModel(InjectAs.Transient)]
    internal class SettingViewModel : ObservableRecipient2,
        IRecipient<UpdateProgressedMessage>,
        IRecipient<AdaptiveBackgroundOpacityChangedMessage>
    {
        private readonly IUpdateService updateService;

        private bool skipCacheCheck;
        private string versionString;
        private string userId;
        private AutoRun autoRun = new();
        private NamedValue<ApplicationTheme?> selectedTheme;
        private bool isTaskBarIconEnabled;
        private bool closeMainWindowAfterInitializaion;
        private UpdateProgressedMessage updateInfo;
        private double backgroundOpacity;
        private bool isBackgroundOpacityAdaptive;
        private bool isBannerWithNoItemVisible;
        private NamedValue<UpdateAPI> currentUpdateAPI;

        /// <summary>
        /// 构造一个新的设置视图模型
        /// </summary>
        /// <param name="updateService">更新服务</param>
        /// <param name="asyncRelayCommandFactory">异步命令工厂</param>
        /// <param name="messenger">消息器</param>
        public SettingViewModel(IUpdateService updateService, IAsyncRelayCommandFactory asyncRelayCommandFactory, IMessenger messenger)
            : base(messenger)
        {
            this.updateService = updateService;

            this.selectedTheme = this.Themes.First(x => x.Value == Setting2.AppTheme);
            this.currentUpdateAPI = this.UpdateAPIs.First(x => x.Value == Setting2.UpdateAPI);

            this.SkipCacheCheck = Setting2.SkipCacheCheck;
            this.IsTaskBarIconEnabled = Setting2.IsTaskBarIconEnabled;
            this.CloseMainWindowAfterInitializaion = Setting2.CloseMainWindowAfterInitializaion;
            this.BackgroundOpacity = Setting2.BackgroundOpacity;
            this.IsBackgroundOpacityAdaptive = Setting2.IsBackgroundOpacityAdaptive;
            this.IsBannerWithNoItemVisible = Setting2.IsBannerWithNoItemVisible;

            Version v = App.Current.Version;
            this.VersionString = $"Snap Genshin {v.Major} - Version {v.Minor}.{v.Build} Build {v.Revision}";
            this.UserId = User.Id;

            this.UpdateInfo = UpdateProgressedMessage.Default;
            this.ReleaseNote = updateService.ReleaseNote;

            this.CheckUpdateCommand = asyncRelayCommandFactory.Create(this.CheckUpdateAsync);
            this.CopyUserIdCommand = new RelayCommand(this.CopyUserIdToClipBoard);

            this.SponsorUICommand = new RelayCommand(this.NavigateToSponsorPage);
            this.OpenBackgroundFolderCommand = new RelayCommand(() => FileExplorer.Open(PathContext.Locate("Background")));
            this.OpenCacheFolderCommand = new RelayCommand(() => FileExplorer.Open(PathContext.Locate("Cache")));
            this.NextWallpaperCommand = new RelayCommand(this.SwitchToNextWallpaper);
            this.OpenImplementationPageCommand = new RelayCommand(() => messenger.Send(new NavigateRequestMessage(typeof(ImplementationPage))));
        }

        /// <summary>
        /// 可选的主题色
        /// </summary>
        public List<NamedValue<ApplicationTheme?>> Themes { get; } = new()
        {
            new("浅色", ApplicationTheme.Light),
            new("深色", ApplicationTheme.Dark),
            new("系统默认", null),
        };

        /// <summary>
        /// 可选的更新通道
        /// </summary>
        public List<NamedValue<UpdateAPI>> UpdateAPIs { get; } = new()
        {
            new("正式通道", UpdateAPI.PatchAPI),
            new("预览通道", UpdateAPI.GithubAPI),
        };

        /// <summary>
        /// 跳过完整性检查
        /// </summary>
        public bool SkipCacheCheck
        {
            get => this.skipCacheCheck;

            set
            {
                Setting2.SkipCacheCheck.Set(value, false);
                this.SetProperty(ref this.skipCacheCheck, value);
            }
        }

        /// <summary>
        /// 是否启用任务栏图标
        /// </summary>
        public bool IsTaskBarIconEnabled
        {
            get => this.isTaskBarIconEnabled;

            set
            {
                Setting2.IsTaskBarIconEnabled.Set(value, false);
                this.SetProperty(ref this.isTaskBarIconEnabled, value);
            }
        }

        /// <summary>
        /// 在初始化完成后关闭主窗体
        /// </summary>
        public bool CloseMainWindowAfterInitializaion
        {
            get => this.closeMainWindowAfterInitializaion;

            set
            {
                Setting2.CloseMainWindowAfterInitializaion.Set(value, false);
                this.SetProperty(ref this.closeMainWindowAfterInitializaion, value);
            }
        }

        /// <summary>
        /// 背景不透明度
        /// </summary>
        public double BackgroundOpacity
        {
            get => this.backgroundOpacity;

            set
            {
                Setting2.BackgroundOpacity.Set(value, false);
                this.Messenger.Send(new BackgroundOpacityChangedMessage(value));
                this.SetProperty(ref this.backgroundOpacity, value);
            }
        }

        /// <summary>
        /// 是否启用自适应背景不透明度
        /// </summary>
        public bool IsBackgroundOpacityAdaptive
        {
            get => this.isBackgroundOpacityAdaptive;

            set
            {
                Setting2.IsBackgroundOpacityAdaptive.Set(value, false);
                this.SetProperty(ref this.isBackgroundOpacityAdaptive, value);
            }
        }

        /// <summary>
        /// 版本字符串
        /// </summary>
        public string VersionString
        {
            get => this.versionString;

            [MemberNotNull("versionString")]
            set => this.SetProperty(ref this.versionString, value);
        }

        /// <summary>
        /// 用户设备ID
        /// </summary>
        public string UserId
        {
            get => this.userId;

            [MemberNotNull(nameof(userId))]
            set => this.userId = value;
        }

        /// <summary>
        /// 是否显示未抽取的卡池
        /// </summary>
        public bool IsBannerWithNoItemVisible
        {
            get => this.isBannerWithNoItemVisible;

            set
            {
                Setting2.IsBannerWithNoItemVisible.Set(value, false);
                this.SetProperty(ref this.isBannerWithNoItemVisible, value);
            }
        }

        /// <summary>
        /// 自启
        /// </summary>
        public AutoRun AutoRun
        {
            get => this.autoRun;

            set => this.autoRun = value;
        }

        /// <summary>
        /// 选中的主题
        /// </summary>
        public NamedValue<ApplicationTheme?> SelectedTheme
        {
            get => this.selectedTheme;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.selectedTheme, value, v => { this.UpdateAppTheme(v!); });
        }

        /// <summary>
        /// 当前的更新通道
        /// </summary>
        public NamedValue<UpdateAPI> CurrentUpdateAPI
        {
            get => this.currentUpdateAPI;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.currentUpdateAPI, value, v => Setting2.UpdateAPI.Set(v.Value));
        }

        /// <summary>
        /// 更新信息
        /// </summary>
        public UpdateProgressedMessage UpdateInfo
        {
            get => this.updateInfo;

            [MemberNotNull(nameof(updateInfo))]
            set => this.SetProperty(ref this.updateInfo, value);
        }

        /// <summary>
        /// 更新日志
        /// </summary>
        public string? ReleaseNote { get; }

        /// <summary>
        /// 检查更新命令
        /// </summary>
        public ICommand CheckUpdateCommand { get; }

        /// <summary>
        /// 复制用户设备ID命令
        /// </summary>
        public ICommand CopyUserIdCommand { get; }

        /// <summary>
        /// 打开赞助页面命令
        /// </summary>
        public ICommand SponsorUICommand { get; }

        /// <summary>
        /// 打开缓存文件夹命令
        /// </summary>
        public ICommand OpenCacheFolderCommand { get; }

        /// <summary>
        /// 打开背景图片文件夹
        /// </summary>
        public ICommand OpenBackgroundFolderCommand { get; }

        /// <summary>
        /// 下一张背景图片命令
        /// </summary>
        public ICommand NextWallpaperCommand { get; }

        /// <summary>
        /// 打开可切换实现页面命令
        /// </summary>
        public ICommand OpenImplementationPageCommand { get; }

        /// <inheritdoc/>
        public void Receive(UpdateProgressedMessage message)
        {
            this.UpdateInfo = message;
        }

        /// <inheritdoc/>
        public void Receive(AdaptiveBackgroundOpacityChangedMessage message)
        {
            this.BackgroundOpacity = message.Value;
        }

        private void CopyUserIdToClipBoard()
        {
            Clipboard.Clear();
            try
            {
                Clipboard.SetText(this.UserId);
            }
            catch
            {
                try
                {
                    Clipboard2.SetText(this.UserId);
                }
                catch
                {
                }
            }
        }

        private void NavigateToSponsorPage()
        {
            this.Messenger.Send(new NavigateRequestMessage(typeof(SponsorPage)));
        }

        private async Task CheckUpdateAsync()
        {
            UpdateState result = await this.updateService.CheckUpdateStateAsync();
            switch (result)
            {
                case UpdateState.NeedUpdate:
                    {
                        await this.updateService.DownloadAndInstallPackageAsync();
                        break;
                    }

                case UpdateState.IsNewestRelease:
                    {
                        new ToastContentBuilder()
                            .AddText("已是最新发行版")
                            .SafeShow();
                        break;
                    }

                case UpdateState.IsInsiderVersion:
                    {
                        new ToastContentBuilder()
                            .AddText("当前为开发测试版")
                            .SafeShow();
                        break;
                    }

                case UpdateState.NotAvailable:
                    {
                        new ToastContentBuilder()
                            .AddText("检查更新失败")
                            .SafeShow();
                        break;
                    }

                default:
                    break;
            }
        }

        private void SwitchToNextWallpaper()
        {
            this.Messenger.Send(new BackgroundChangeRequestMessage());
        }

        [PropertyChangedCallback]
        private void UpdateAppTheme(NamedValue<ApplicationTheme?> value)
        {
            Setting2.AppTheme.Set(value.Value);
            ThemeManager.Current.ApplicationTheme = Setting2.AppTheme;
        }
    }
}