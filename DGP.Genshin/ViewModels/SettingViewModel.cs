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
using System.Threading.Tasks;

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
        private readonly IUpdateService updateService;

        public List<NamedValue<ApplicationTheme?>> Themes { get; } = new()
        {
            new("浅色", ApplicationTheme.Light),
            new("深色", ApplicationTheme.Dark),
            new("系统默认", null),
        };

        private bool showFullUID;
        private bool autoDailySignInOnLaunch;
        private bool skipCacheCheck;
        private bool signInSilently;

        private string versionString;
        private AutoRun autoRun = new();
        private NamedValue<ApplicationTheme?> selectedTheme;
        private IAsyncRelayCommand checkUpdateCommand;

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

        public string VersionString
        {
            get => versionString;
            [MemberNotNull("versionString")]
            set => SetProperty(ref versionString, value);
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
        public IAsyncRelayCommand CheckUpdateCommand
        {
            get => checkUpdateCommand;
            [MemberNotNull(nameof(checkUpdateCommand))]
            set => checkUpdateCommand = value;
        }

        public SettingViewModel(ISettingService settingService, IUpdateService updateService, IMessenger messenger) : base(messenger)
        {
            this.settingService = settingService;
            this.updateService = updateService;
            SelectedTheme = Themes.First(x => x.Value == SettingService.GetOrDefault(Setting.AppTheme, null, Setting.ApplicationThemeConverter));
            CheckUpdateCommand = new AsyncRelayCommand(updateService.CheckUpdateStateAsync);
            Initialize();
            IsActive = true;
        }

        [MemberNotNull("versionString")]
        private void Initialize()
        {
            showFullUID = SettingService.GetOrDefault(Setting.ShowFullUID, false);
            autoDailySignInOnLaunch = SettingService.GetOrDefault(Setting.AutoDailySignInOnLaunch, false);
            skipCacheCheck = SettingService.GetOrDefault(Setting.SkipCacheCheck, false);
            signInSilently = SettingService.GetOrDefault(Setting.SignInSilently, false);

            //version
            Version v = Assembly.GetExecutingAssembly().GetName().Version!;
            VersionString = $"DGP.Genshin - version {v.Major}.{v.Minor}.{v.Build} Build {v.Revision}";
        }

        public async Task CheckUpdateAsync()
        {
            UpdateState result = await updateService.CheckUpdateStateAsync();
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
                default:
                    break;
            }
        }
    }
}
