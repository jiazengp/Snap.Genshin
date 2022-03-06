using DGP.Genshin.Core.Notification;
using DGP.Genshin.Helper;
using DGP.Genshin.Message;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstraction;
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
    public class SettingViewModel : ObservableRecipient2, IRecipient<UpdateProgressedMessage>
    {
        private readonly IUpdateService updateService;

        #region Observable
        public List<NamedValue<ApplicationTheme?>> Themes { get; } = new()
        {
            new("浅色", ApplicationTheme.Light),
            new("深色", ApplicationTheme.Dark),
            new("系统默认", null),
        };
        public List<NamedValue<TimeSpan>> ResinAutoRefreshTime
        {
            get
            {
                return new()
                {
                    new("4 分钟 | 0.5 树脂", TimeSpan.FromMinutes(4)),
                    new("8 分钟 | 1 树脂", TimeSpan.FromMinutes(8)),
                    new("30 分钟 | 3.75 树脂", TimeSpan.FromMinutes(30)),
                    new("40 分钟 | 5 树脂", TimeSpan.FromMinutes(40)),
                    new("1 小时 | 7.5 树脂", TimeSpan.FromMinutes(60))
                };
            }
        }

        private bool autoDailySignInOnLaunch;
        private bool skipCacheCheck;
        private bool signInSilently;
        private bool updateUseFastGit;
        private string versionString;
        private string userId;
        private AutoRun autoRun = new();
        private NamedValue<ApplicationTheme?> selectedTheme;
        private NamedValue<TimeSpan> selectedResinAutoRefreshTime;
        private bool isTaskBarIconEnabled;
        private bool closeMainWindowAfterInitializaion;
        private UpdateProgressedMessage updateInfo;
        private double backgroundOpacity;
        private bool isBackgroundOpacityAdaptive;
        private bool isBannerWithNoItemVisible;

        public bool AutoDailySignInOnLaunch
        {
            get => autoDailySignInOnLaunch;

            set
            {
                Setting2.AutoDailySignInOnLaunch.Set(value, false);
                SetProperty(ref autoDailySignInOnLaunch, value);
            }
        }
        public bool SkipCacheCheck
        {
            get => skipCacheCheck;

            set
            {
                Setting2.SkipCacheCheck.Set(value, false);
                SetProperty(ref skipCacheCheck, value);
            }
        }
        public bool SignInSilently
        {
            get => signInSilently;

            set
            {
                Setting2.SignInSilently.Set(value, false);
                SetProperty(ref signInSilently, value);
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
                Setting2.BackgroundOpacity.Set(value, false, false);
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
            ThemeManager.Current.ApplicationTheme = Setting2.AppTheme.Get();
        }
        public NamedValue<TimeSpan> SelectedResinAutoRefreshTime
        {
            get => selectedResinAutoRefreshTime;

            set
            {
                SetPropertyAndCallbackOnCompletion(ref selectedResinAutoRefreshTime, value,
              v => Setting2.ResinRefreshMinutes.Set(v!.Value.TotalMinutes));
            }
        }
        public UpdateProgressedMessage UpdateInfo
        {
            get => updateInfo;

            [MemberNotNull(nameof(updateInfo))]
            set => SetProperty(ref updateInfo, value);
        }
        #endregion

        public ICommand CheckUpdateCommand { get; }
        public ICommand CopyUserIdCommand { get; }
        public ICommand SignInImmediatelyCommand { get; }
        public ICommand SponsorUICommand { get; }
        public ICommand OpenCacheFolderCommand { get; }
        public ICommand OpenBackgroundFolderCommand { get; }

        public SettingViewModel(IUpdateService updateService, ICookieService cookieService, ISignInService signInService, IMessenger messenger) : base(messenger)
        {
            this.updateService = updateService;

            ApplicationTheme? theme = Setting2.AppTheme.Get();
            selectedTheme = Themes.First(x => x.Value == theme);

            double minutes = Setting2.ResinRefreshMinutes.Get();
            selectedResinAutoRefreshTime = ResinAutoRefreshTime.First(s => s.Value.TotalMinutes == minutes)!;

            AutoDailySignInOnLaunch = Setting2.AutoDailySignInOnLaunch.Get();
            SkipCacheCheck = Setting2.SkipCacheCheck.Get();
            SignInSilently = Setting2.SignInSilently.Get();
            UpdateUseFastGit = Setting2.UpdateUseFastGit.Get();
            IsTaskBarIconEnabled = Setting2.IsTaskBarIconEnabled.Get();
            CloseMainWindowAfterInitializaion = Setting2.CloseMainWindowAfterInitializaion.Get();
            BackgroundOpacity = Setting2.BackgroundOpacity.Get();
            IsBackgroundOpacityAdaptive = Setting2.IsBackgroundOpacityAdaptive.Get();
            IsBannerWithNoItemVisible = Setting2.IsBannerWithNoItemVisible.Get();

            Version v = App.Current.Version;
            VersionString = $"DGP.Genshin - version {v.Major}.{v.Minor}.{v.Build} Build {v.Revision}";
            UserId = User.Id;

            UpdateInfo = UpdateProgressedMessage.Default;

            CheckUpdateCommand = new AsyncRelayCommand(CheckUpdateAsync);
            CopyUserIdCommand = new RelayCommand(CopyUserIdToClipBoard);
            SignInImmediatelyCommand = new AsyncRelayCommand(signInService.TrySignAllAccountsRolesInAsync);
            SponsorUICommand = new RelayCommand(NavigateToSponsorPage);
            OpenBackgroundFolderCommand = new RelayCommand(() => FileExplorer.Open(PathContext.Locate("Background")));
            OpenCacheFolderCommand = new RelayCommand(() => FileExplorer.Open(PathContext.Locate("Cache")));
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

        public void Receive(UpdateProgressedMessage message)
        {
            UpdateInfo = message;
        }
    }
}
