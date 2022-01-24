using DGP.Genshin.Helper;
using DGP.Genshin.Message;
using DGP.Genshin.Page;
using DGP.Genshin.Service.Abstratcion;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using ModernWpf;
using Snap.Core.DependencyInjection;
using Snap.Core.Logging;
using Snap.Core.Mvvm;
using Snap.Data.Primitive;
using Snap.Win32;
using System;
using System.Collections.Generic;
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
    /// 仅供 <see cref="Page.SettingsPage"/> 使用
    /// </summary>
    [ViewModel(InjectAs.Singleton)]
    public class SettingViewModel : ObservableRecipient2, IRecipient<SettingChangedMessage>, IRecipient<UpdateProgressedMessage>
    {
        private readonly ISettingService settingService;
        private readonly IUpdateService updateService;
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

        private bool showFullUID;
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

        #region Need Initalize
        //需要在 Initalize Receive 中添加字段的初始化
        public bool ShowFullUID
        {
            get => showFullUID; set
            {
                SettingService.SetValueNoNotify(Setting.ShowFullUID, value);
                SetProperty(ref showFullUID, value);
            }
        }
        public bool AutoDailySignInOnLaunch
        {
            get => autoDailySignInOnLaunch; set
            {
                SettingService.SetValueNoNotify(Setting.AutoDailySignInOnLaunch, value);
                SetProperty(ref autoDailySignInOnLaunch, value);
            }
        }
        public bool SkipCacheCheck
        {
            get => skipCacheCheck; set
            {
                SettingService.SetValueNoNotify(Setting.SkipCacheCheck, value);
                SetProperty(ref skipCacheCheck, value);
            }
        }
        public bool SignInSilently
        {
            get => signInSilently; set
            {
                SettingService.SetValueNoNotify(Setting.SignInSilently, value);
                SetProperty(ref signInSilently, value);
            }
        }
        public bool UpdateUseFastGit
        {
            get => updateUseFastGit; set
            {
                SettingService.SetValueNoNotify(Setting.UpdateUseFastGit, value);
                SetProperty(ref updateUseFastGit, value);
            }
        }
        public bool IsTaskBarIconEnabled
        {
            get => isTaskBarIconEnabled;
            set
            {
                SettingService.SetValueNoNotify(Setting.IsTaskBarIconEnabled, value);
                SetProperty(ref isTaskBarIconEnabled, value);
            }
        }
        public bool CloseMainWindowAfterInitializaion
        {
            get => closeMainWindowAfterInitializaion;
            set
            {
                SettingService.SetValueNoNotify(Setting.CloseMainWindowAfterInitializaion, value);
                SetProperty(ref closeMainWindowAfterInitializaion, value);
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
            SettingService[Setting.AppTheme] = value.Value;
            ThemeManager.Current.ApplicationTheme = SettingService.GetOrDefault(Setting.AppTheme, null, Setting.ApplicationThemeConverter);
        }
        public NamedValue<TimeSpan> SelectedResinAutoRefreshTime
        {
            get => selectedResinAutoRefreshTime;
            set => SetPropertyAndCallbackOnCompletion(ref selectedResinAutoRefreshTime, value,
                v => settingService[Setting.ResinRefreshMinutes] = v!.Value.TotalMinutes);
        }
        public UpdateProgressedMessage UpdateInfo
        {
            get => updateInfo;
            [MemberNotNull(nameof(updateInfo))]
            set => SetProperty(ref updateInfo, value);
        }

        public ICommand CheckUpdateCommand { get; }
        public ICommand CopyUserIdCommand { get; }
        public ICommand SignInImmediatelyCommand { get; }
        public ICommand SponsorUICommand { get; }
        #endregion

        public SettingViewModel(ISettingService settingService, IUpdateService updateService, IMessenger messenger) : base(messenger)
        {
            this.settingService = settingService;
            this.updateService = updateService;

            ApplicationTheme? theme = settingService.GetOrDefault(Setting.AppTheme, null, Setting.ApplicationThemeConverter);
            selectedTheme = Themes.First(x => x.Value == theme);

            double minutes = settingService.GetOrDefault(Setting.ResinRefreshMinutes, 8.0);
            selectedResinAutoRefreshTime = ResinAutoRefreshTime.First(s => s.Value.TotalMinutes == minutes)!;

            Initialize();

            UpdateInfo = UpdateProgressedMessage.Default;

            CheckUpdateCommand = new AsyncRelayCommand(CheckUpdateAsync);
            CopyUserIdCommand = new RelayCommand(CopyUserIdToClipBoard);
            SignInImmediatelyCommand = new AsyncRelayCommand(MainWindow.SignInAllAccountsRolesAsync);
            SponsorUICommand = new RelayCommand(NavigateToSponsorPage);
        }

        [MemberNotNull(nameof(versionString)), MemberNotNull(nameof(userId))]
        private void Initialize()
        {
            //不能直接设置属性 会导致触发通知操作进而造成死循环
            showFullUID = SettingService.GetOrDefault(Setting.ShowFullUID, false);
            autoDailySignInOnLaunch = SettingService.GetOrDefault(Setting.AutoDailySignInOnLaunch, false);
            skipCacheCheck = SettingService.GetOrDefault(Setting.SkipCacheCheck, false);
            signInSilently = SettingService.GetOrDefault(Setting.SignInSilently, false);
            updateUseFastGit = SettingService.GetOrDefault(Setting.UpdateUseFastGit, false);
            isTaskBarIconEnabled = SettingService.GetOrDefault(Setting.IsTaskBarIconEnabled, true);
            closeMainWindowAfterInitializaion = SettingService.GetOrDefault(Setting.CloseMainWindowAfterInitializaion, false);

            //version
            Version v = Assembly.GetExecutingAssembly().GetName().Version!;
            VersionString = $"DGP.Genshin - version {v.Major}.{v.Minor}.{v.Build} Build {v.Revision}";
            UserId = User.Id;
        }

        private void CopyUserIdToClipBoard()
        {
            Clipboard.Clear();
            Clipboard2.SetText(UserId);
        }
        private void NavigateToSponsorPage()
        {
            App.Messenger.Send(new NavigateRequestMessage(typeof(SponsorPage)));
        }
        private async Task CheckUpdateAsync()
        {
            UpdateState result = await updateService.CheckUpdateStateAsync();
            //update debug code here
            //result = UpdateState.NeedUpdate;
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

        /// <summary>
        /// 当以编程形式改变设置时触发
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Receive(SettingChangedMessage message)
        {
            string key = message.Value.Key;
            object? value = message.Value.Value;

            this.Log($"setting {key} changed: {value}");
            switch (key)
            {
                case Setting.ShowFullUID:
                    ShowFullUID = value is not null && (bool)value;
                    break;
                case Setting.AutoDailySignInOnLaunch:
                    AutoDailySignInOnLaunch = value is not null && (bool)value;
                    break;
                case Setting.SkipCacheCheck:
                    SkipCacheCheck = value is not null && (bool)value;
                    break;
                case Setting.SignInSilently:
                    SignInSilently = value is not null && (bool)value;
                    break;
                case Setting.UpdateUseFastGit:
                    UpdateUseFastGit = value is not null && (bool)value;
                    break;
                case Setting.IsTaskBarIconEnabled:
                    IsTaskBarIconEnabled = value is not null && (bool)value;
                    break;
                case Setting.CloseMainWindowAfterInitializaion:
                    CloseMainWindowAfterInitializaion = value is not null && (bool)value;
                    break;
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
