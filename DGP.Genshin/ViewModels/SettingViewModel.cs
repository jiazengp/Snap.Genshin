using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Data.Behavior;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Helpers;
using DGP.Genshin.Messages;
using DGP.Genshin.Services.Abstratcions;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using ModernWpf;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace DGP.Genshin.ViewModels
{
    /// <summary>
    /// 为需要及时响应的设置项提供 <see cref="Observable"/> 模型支持
    /// 仅供 <see cref="Pages.SettingsPage"/> 使用
    /// </summary>
    [ViewModel(ViewModelType.Singleton)]
    public class SettingViewModel : ObservableRecipient, IRecipient<SettingChangedMessage>
    {
        private readonly ISettingService settingService;
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
        private IAsyncRelayCommand checkUpdateCommand;
        private bool isTaskBarIconEnabled;
        private bool closeMainWindowAfterInitializaion;
        private ICommand copyUserIdCommand;

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
                isTaskBarIconEnabled = value;
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
            [MemberNotNull(nameof(selectedTheme))]
            set
            {
                SetProperty(ref selectedTheme, value);
                SettingService[Setting.AppTheme] = value.Value;
                UpdateAppTheme();
            }
        }
        internal void UpdateAppTheme()
        {
            ThemeManager.Current.ApplicationTheme = SettingService.GetOrDefault(Setting.AppTheme, null, Setting.ApplicationThemeConverter);
        }
        public NamedValue<TimeSpan> SelectedResinAutoRefreshTime
        {
            get => selectedResinAutoRefreshTime;
            [MemberNotNull(nameof(selectedResinAutoRefreshTime))]
            set
            {
                SetProperty(ref selectedResinAutoRefreshTime, value);
                settingService[Setting.ResinRefreshMinutes] = value.Value.TotalMinutes;
            }
        }
        public IAsyncRelayCommand CheckUpdateCommand
        {
            get => checkUpdateCommand;
            [MemberNotNull(nameof(checkUpdateCommand))]
            set => checkUpdateCommand = value;
        }
        public ICommand CopyUserIdCommand
        {
            get => copyUserIdCommand;
            [MemberNotNull(nameof(copyUserIdCommand))]
            set => copyUserIdCommand = value;
        }
        #endregion

        public SettingViewModel(ISettingService settingService, IUpdateService updateService, IMessenger messenger) : base(messenger)
        {
            this.settingService = settingService;

            ApplicationTheme? theme = settingService.GetOrDefault(Setting.AppTheme, null, Setting.ApplicationThemeConverter);
            selectedTheme = Themes.First(x => x.Value == theme);

            double minutes = settingService.GetOrDefault(Setting.ResinRefreshMinutes, 8.0);
            selectedResinAutoRefreshTime = ResinAutoRefreshTime.First(s => s.Value.TotalMinutes == minutes)!;

            CheckUpdateCommand = new AsyncRelayCommand(updateService.CheckUpdateStateAsync);
            CopyUserIdCommand = new RelayCommand(CopyUserIdToClipBoard);

            Initialize();
            IsActive = true;
        }

        ~SettingViewModel()
        {
            IsActive = false;
        }

        [MemberNotNull(nameof(versionString))]
        [MemberNotNull(nameof(userId))]
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
            Clipboard.SetText(UserId);
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
    }
}
