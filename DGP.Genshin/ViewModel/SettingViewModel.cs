using DGP.Genshin.Core.Notification;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Helper;
using DGP.Genshin.Message;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstraction.Setting;
using DGP.Genshin.Service.Abstraction.Updating;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using ModernWpf;
using Snap.Core.DependencyInjection;
using Snap.Core.Mvvm;
using Snap.Data.Primitive;
using Snap.Data.Utility;
using Snap.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        #region Observable
        public List<NamedValue<ApplicationTheme?>> Themes { get; } = new()
        {
            new("浅色", ApplicationTheme.Light),
            new("深色", ApplicationTheme.Dark),
            new("系统默认", null),
        };
        public List<NamedValue<UpdateAPI>> UpdateAPIs { get; } = new()
        {
            new("正式通道", UpdateAPI.PatchAPI),
            new("预览通道", UpdateAPI.GithubAPI)
        };


        private bool skipCacheCheck;

        private bool updateUseFastGit;
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


        public bool SkipCacheCheck
        {
            get => this.skipCacheCheck;

            set
            {
                Setting2.SkipCacheCheck.Set(value, false);
                this.SetProperty(ref this.skipCacheCheck, value);
            }
        }
        public bool UpdateUseFastGit
        {
            get => this.updateUseFastGit;

            set
            {
                Setting2.UpdateUseFastGit.Set(value, false);
                this.SetProperty(ref this.updateUseFastGit, value);
            }
        }
        public bool IsTaskBarIconEnabled
        {
            get => this.isTaskBarIconEnabled;

            set
            {
                Setting2.IsTaskBarIconEnabled.Set(value, false);
                this.SetProperty(ref this.isTaskBarIconEnabled, value);
            }
        }
        public bool CloseMainWindowAfterInitializaion
        {
            get => this.closeMainWindowAfterInitializaion;

            set
            {
                Setting2.CloseMainWindowAfterInitializaion.Set(value, false);
                this.SetProperty(ref this.closeMainWindowAfterInitializaion, value);
            }
        }
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
        public bool IsBackgroundOpacityAdaptive
        {
            get => this.isBackgroundOpacityAdaptive;

            set
            {
                Setting2.IsBackgroundOpacityAdaptive.Set(value, false);
                this.SetProperty(ref this.isBackgroundOpacityAdaptive, value);
            }
        }
        public string VersionString
        {
            get => this.versionString;

            [MemberNotNull("versionString")]
            set => this.SetProperty(ref this.versionString, value);
        }
        public string UserId
        {
            get => this.userId;

            [MemberNotNull(nameof(userId))]
            set => this.userId = value;
        }
        public bool IsBannerWithNoItemVisible
        {
            get => this.isBannerWithNoItemVisible;

            set
            {
                Setting2.IsBannerWithNoItemVisible.Set(value, false);
                this.SetProperty(ref this.isBannerWithNoItemVisible, value);
            }
        }
        public AutoRun AutoRun
        {
            get => this.autoRun;

            set => this.autoRun = value;
        }
        public NamedValue<ApplicationTheme?> SelectedTheme
        {
            get => this.selectedTheme;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.selectedTheme, value, v => { this.UpdateAppTheme(v!); });
        }
        [PropertyChangedCallback]
        private void UpdateAppTheme(NamedValue<ApplicationTheme?> value)
        {
            Setting2.AppTheme.Set(value.Value);
            ThemeManager.Current.ApplicationTheme = Setting2.AppTheme;
        }

        public NamedValue<UpdateAPI> CurrentUpdateAPI
        {
            get => this.currentUpdateAPI;

            set => this.SetPropertyAndCallbackOnCompletion(ref this.currentUpdateAPI, value, v => Setting2.UpdateAPI.Set(v.Value));
        }
        public UpdateProgressedMessage UpdateInfo
        {
            get => this.updateInfo;

            [MemberNotNull(nameof(updateInfo))]
            set => this.SetProperty(ref this.updateInfo, value);
        }
        public string? ReleaseNote { get; }
        #endregion

        public ICommand CheckUpdateCommand { get; }
        public ICommand CopyUserIdCommand { get; }
        public ICommand SponsorUICommand { get; }
        public ICommand OpenCacheFolderCommand { get; }
        public ICommand OpenBackgroundFolderCommand { get; }
        public ICommand NextWallpaperCommand { get; }
        public ICommand OpenImplementationPageCommand { get; }

        public SettingViewModel(IUpdateService updateService, IAsyncRelayCommandFactory asyncRelayCommandFactory, IMessenger messenger) : base(messenger)
        {
            this.updateService = updateService;

            this.selectedTheme = this.Themes.First(x => x.Value == Setting2.AppTheme);
            this.currentUpdateAPI = this.UpdateAPIs.First(x => x.Value == Setting2.UpdateAPI);

            this.SkipCacheCheck = Setting2.SkipCacheCheck;
            this.UpdateUseFastGit = Setting2.UpdateUseFastGit;
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
                catch { }
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

        public void Receive(UpdateProgressedMessage message)
        {
            this.UpdateInfo = message;
        }
        public void Receive(AdaptiveBackgroundOpacityChangedMessage message)
        {
            this.BackgroundOpacity = message.Value;
        }
    }
}
