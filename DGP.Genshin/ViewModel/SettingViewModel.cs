using DGP.Genshin.Helper;
using DGP.Genshin.Message;
using DGP.Genshin.MiHoYoAPI.Record.DailyNote;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstraction;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using ModernWpf;
using ModernWpf.Controls;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using Snap.Data.Primitive;
using Snap.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DGP.Genshin.ViewModel
{
    /// <summary>
    /// 为需要及时响应的设置项提供 <see cref="Observable"/> 模型支持
    /// </summary>
    [ViewModel(InjectAs.Singleton)]
    public class SettingViewModel : ObservableRecipient2, IRecipient<UpdateProgressedMessage>
    {
        private readonly ISettingService settingService;
        private readonly IUpdateService updateService;
        private readonly ICookieService cookieService;
        private ISettingService SettingService => settingService;

        public List<NamedValue<ApplicationTheme?>> Themes { get; } = new()
        {
            new("浅色", ApplicationTheme.Light),
            new("深色", ApplicationTheme.Dark),
            new("系统默认", null),
        };
        public List<NamedValue<TimeSpan>> ResinAutoRefreshTime => new()
        {
            new("8 分钟 | 1 树脂", TimeSpan.FromMinutes(8)),
            new("30 分钟 | 3.75 树脂", TimeSpan.FromMinutes(30)),
            new("40 分钟 | 5 树脂", TimeSpan.FromMinutes(40)),
            new("1 小时 | 7.5 树脂", TimeSpan.FromMinutes(60))
        };

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

        #region Need Initalize
        //需要在 Initalize Receive 中添加字段的初始化
        public bool AutoDailySignInOnLaunch
        {
            get => autoDailySignInOnLaunch; set
            {
                Setting2.AutoDailySignInOnLaunch.Set(value, false);
                SetProperty(ref autoDailySignInOnLaunch, value);
            }
        }
        public bool SkipCacheCheck
        {
            get => skipCacheCheck; set
            {
                Setting2.SkipCacheCheck.Set(value, false);
                SetProperty(ref skipCacheCheck, value);
            }
        }
        public bool SignInSilently
        {
            get => signInSilently; set
            {
                Setting2.SignInSilently.Set(value, false);
                SetProperty(ref signInSilently, value);
            }
        }
        public bool UpdateUseFastGit
        {
            get => updateUseFastGit; set
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
                App.Messenger.Send(new BackgroundOpacityChangedMessage(value));
                SetProperty(ref backgroundOpacity, value);
            }
        }
        #endregion

        #region Not Need
        //不需要显式初始化的字段
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
        public AutoRun AutoRun { get => autoRun; set => autoRun = value; }
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
            set => SetPropertyAndCallbackOnCompletion(ref selectedResinAutoRefreshTime, value,
                v => Setting2.ResinRefreshMinutes.Set(v!.Value.TotalMinutes));
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
        public ICommand EnableDailyNoteCommand { get; }

        public SettingViewModel(ISettingService settingService, IUpdateService updateService, ICookieService cookieService, IMessenger messenger) : base(messenger)
        {
            this.settingService = settingService;
            this.updateService = updateService;
            this.cookieService = cookieService;

            ApplicationTheme? theme = Setting2.AppTheme.Get();
            selectedTheme = Themes.First(x => x.Value == theme);

            double minutes = Setting2.ResinRefreshMinutes.Get();
            selectedResinAutoRefreshTime = ResinAutoRefreshTime.First(s => s.Value.TotalMinutes == minutes)!;

            Initialize();

            UpdateInfo = UpdateProgressedMessage.Default;

            CheckUpdateCommand = new AsyncRelayCommand(CheckUpdateAsync);
            CopyUserIdCommand = new RelayCommand(CopyUserIdToClipBoard);
            SignInImmediatelyCommand = new AsyncRelayCommand(MainWindow.SignInAllAccountsRolesAsync);
            SponsorUICommand = new RelayCommand(NavigateToSponsorPage);
            OpenBackgroundFolderCommand = new RelayCommand(() => Process.Start("explorer.exe", PathContext.Locate("Background")));
            OpenCacheFolderCommand = new RelayCommand(() => Process.Start("explorer.exe", PathContext.Locate("Cache")));
            EnableDailyNoteCommand = new AsyncRelayCommand(EnableDailyNotePermissionAsync);
        }

        [MemberNotNull(nameof(versionString)), MemberNotNull(nameof(userId))]
        private void Initialize()
        {
            //不能直接设置属性 会导致触发通知操作进而造成死循环
            autoDailySignInOnLaunch = Setting2.AutoDailySignInOnLaunch.Get();
            skipCacheCheck = Setting2.SkipCacheCheck.Get();
            signInSilently = Setting2.SignInSilently.Get();
            updateUseFastGit = Setting2.UpdateUseFastGit.Get();
            isTaskBarIconEnabled = Setting2.IsTaskBarIconEnabled.Get();
            closeMainWindowAfterInitializaion = Setting2.CloseMainWindowAfterInitializaion.Get();
            backgroundOpacity = Setting2.BackgroundOpacity.Get();

            //version
            Version v = Assembly.GetExecutingAssembly().GetName().Version!;
            VersionString = $"DGP.Genshin - version {v.Major}.{v.Minor}.{v.Build} Build {v.Revision}";
            UserId = User.Id;
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
            App.Messenger.Send(new NavigateRequestMessage(typeof(SponsorPage)));
        }
        private async Task CheckUpdateAsync()
        {
            UpdateState result = await updateService.CheckUpdateStateAsync();
#if DEBUG
            //update debug code here
            result = UpdateState.NeedUpdate;
#endif
            switch (result)
            {
                case UpdateState.NeedUpdate:
                    {
                        await updateService.DownloadAndInstallPackageAsync();
                        break;
                    }
                default:
                    break;
            }
        }
        private async Task EnableDailyNotePermissionAsync()
        {
            object? result = await new DailyNoteProvider(cookieService.CurrentCookie).ChangeDailyNoteDataSwitchAsync(true);
            await new ContentDialog()
            {
                Title = result is null ? "操作失败" : "操作成功",
                PrimaryButtonText = "确定",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
        }

        public void Receive(UpdateProgressedMessage message)
        {
            UpdateInfo = message;
        }
    }
}
