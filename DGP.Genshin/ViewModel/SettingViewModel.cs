using DGP.Genshin.Core.Notification;
using DGP.Genshin.Factory.Abstraction;
using DGP.Genshin.Helper;
using DGP.Genshin.Message;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstraction;
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
            get => skipCacheCheck;

            set
            {
                Setting2.SkipCacheCheck.Set(value, false);
                SetProperty(ref skipCacheCheck, value);
            }
        }
        public bool UpdateUseFastGit
        {
            get => updateUseFastGit;

            set
            {
                Setting2.UpdateUseFastGit.Set(value, false);
                SetProperty(ref updateUseFastGit, value);
            }
        }
        public bool IsTaskBarIconEnabled
        {
            get => isTaskBarIconEnabled;

            set
            {
                Setting2.IsTaskBarIconEnabled.Set(value, false);
                SetProperty(ref isTaskBarIconEnabled, value);
            }
        }
        public bool CloseMainWindowAfterInitializaion
        {
            get => closeMainWindowAfterInitializaion;

            set
            {
                Setting2.CloseMainWindowAfterInitializaion.Set(value, false);
                SetProperty(ref closeMainWindowAfterInitializaion, value);
            }
        }
        public double BackgroundOpacity
        {
            get => backgroundOpacity;

            set
            {
                Setting2.BackgroundOpacity.Set(value, false);
                Messenger.Send(new BackgroundOpacityChangedMessage(value));
                SetProperty(ref backgroundOpacity, value);
            }
        }
        public bool IsBackgroundOpacityAdaptive
        {
            get => isBackgroundOpacityAdaptive;

            set
            {
                Setting2.IsBackgroundOpacityAdaptive.Set(value, false);
                SetProperty(ref isBackgroundOpacityAdaptive, value);
            }
        }
        public string VersionString
        {
            get => versionString;

            [MemberNotNull("versionString")]
            set => SetProperty(ref versionString, value);
        }
        public string UserId
        {
            get => userId;

            [MemberNotNull(nameof(userId))]
            set => userId = value;
        }
        public bool IsBannerWithNoItemVisible
        {
            get => isBannerWithNoItemVisible;

            set
            {
                Setting2.IsBannerWithNoItemVisible.Set(value, false);
                SetProperty(ref isBannerWithNoItemVisible, value);
            }
        }
        public AutoRun AutoRun
        {
            get => autoRun;

            set => autoRun = value;
        }
        public NamedValue<ApplicationTheme?> SelectedTheme
        {
            get => selectedTheme;

            set => SetPropertyAndCallbackOnCompletion(ref selectedTheme, value, v => { UpdateAppTheme(v!); });
        }
        [PropertyChangedCallback]
        private void UpdateAppTheme(NamedValue<ApplicationTheme?> value)
        {
            Setting2.AppTheme.Set(value.Value);
            ThemeManager.Current.ApplicationTheme = Setting2.AppTheme;
        }

        public NamedValue<UpdateAPI> CurrentUpdateAPI
        {
            get => currentUpdateAPI;

            set => SetPropertyAndCallbackOnCompletion(ref currentUpdateAPI, value, v => Setting2.UpdateAPI.Set(v.Value));
        }
        public UpdateProgressedMessage UpdateInfo
        {
            get => updateInfo;

            [MemberNotNull(nameof(updateInfo))]
            set => SetProperty(ref updateInfo, value);
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

        public SettingViewModel(IUpdateService updateService, ICookieService cookieService, IAsyncRelayCommandFactory asyncRelayCommandFactory, IMessenger messenger) : base(messenger)
        {
            this.updateService = updateService;

            selectedTheme = Themes.First(x => x.Value == Setting2.AppTheme);
            currentUpdateAPI = UpdateAPIs.First(x => x.Value == Setting2.UpdateAPI);

            SkipCacheCheck = Setting2.SkipCacheCheck;
            UpdateUseFastGit = Setting2.UpdateUseFastGit;
            IsTaskBarIconEnabled = Setting2.IsTaskBarIconEnabled;
            CloseMainWindowAfterInitializaion = Setting2.CloseMainWindowAfterInitializaion;
            BackgroundOpacity = Setting2.BackgroundOpacity;
            IsBackgroundOpacityAdaptive = Setting2.IsBackgroundOpacityAdaptive;
            IsBannerWithNoItemVisible = Setting2.IsBannerWithNoItemVisible;

            Version v = App.Current.Version;
            VersionString = $"Snap Genshin {v.Major} - Version {v.Minor}.{v.Build} Build {v.Revision}";
            UserId = User.Id;

            UpdateInfo = UpdateProgressedMessage.Default;
            ReleaseNote = updateService.ReleaseNote;

            CheckUpdateCommand = asyncRelayCommandFactory.Create(CheckUpdateAsync);
            CopyUserIdCommand = new RelayCommand(CopyUserIdToClipBoard);

            SponsorUICommand = new RelayCommand(NavigateToSponsorPage);
            OpenBackgroundFolderCommand = new RelayCommand(() => FileExplorer.Open(PathContext.Locate("Background")));
            OpenCacheFolderCommand = new RelayCommand(() => FileExplorer.Open(PathContext.Locate("Cache")));
            NextWallpaperCommand = new RelayCommand(SwitchToNextWallpaper);
            OpenImplementationPageCommand = new RelayCommand(() => messenger.Send(new NavigateRequestMessage(typeof(ImplementationPage))));
        }

        private void CopyUserIdToClipBoard()
        {
            Clipboard.Clear();
            try
            {
                Clipboard.SetText(UserId);
            }
            catch
            {
                try
                {
                    Clipboard2.SetText(UserId);
                }
                catch { }
            }
        }
        private void NavigateToSponsorPage()
        {
            Messenger.Send(new NavigateRequestMessage(typeof(SponsorPage)));
        }
        private async Task CheckUpdateAsync()
        {
            UpdateState result = await updateService.CheckUpdateStateAsync();
            switch (result)
            {
                case UpdateState.NeedUpdate:
                    {
                        await updateService.DownloadAndInstallPackageAsync();
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
            Messenger.Send(new BackgroundChangeRequestMessage());
        }

        public void Receive(UpdateProgressedMessage message)
        {
            UpdateInfo = message;
        }
        public void Receive(AdaptiveBackgroundOpacityChangedMessage message)
        {
            BackgroundOpacity = message.Value;
        }
    }
}
