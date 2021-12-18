﻿using DGP.Genshin.Common.Core.DependencyInjection;
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

namespace DGP.Genshin.ViewModels
{
    /// <summary>
    /// 为需要及时响应的设置项提供 <see cref="Observable"/> 模型支持
    /// 仅供 <see cref="Pages.SettingsPage"/> 使用
    /// </summary>
    [ViewModel(ViewModelType.Singleton)]
    public class SettingViewModel : ObservableObject, IRecipient<SettingChangedMessage>
    {
        private readonly ISettingService settingService;

        public List<NamedValue<ApplicationTheme?>> Themes { get; } = new()
        {
            new("浅色", ApplicationTheme.Light),
            new("深色", ApplicationTheme.Dark),
            new("系统默认", null),
        };

        public SettingViewModel(ISettingService settingService, IUpdateService updateService)
        {
            this.settingService = settingService;
            SelectedTheme = Themes.First(x => x.Value == SettingService.GetOrDefault(Setting.AppTheme, null, Setting.ApplicationThemeConverter));
            CheckUpdateCommand = new AsyncRelayCommand(updateService.CheckUpdateStateAsync);
            Initialize();
        }

        #region Setting Related Property
        private bool showFullUID;
        private bool autoDailySignInOnLaunch;
        private bool skipCacheCheck;
        private bool signInSilently;
        private bool bypassCharactersLimit;

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
        [Obsolete("绕过方法已经不再有效")]
        public bool BypassCharactersLimit
        {
            get => bypassCharactersLimit; set
            {
                SettingService.SetValueNoNotify(Setting.BypassCharactersLimit, value);
                SetProperty(ref bypassCharactersLimit, value);
            }
        }
        #endregion

        [MemberNotNull("versionString")]
        private void Initialize()
        {
            showFullUID = SettingService.GetOrDefault(Setting.ShowFullUID, false);
            autoDailySignInOnLaunch = SettingService.GetOrDefault(Setting.AutoDailySignInOnLaunch, false);
            skipCacheCheck = SettingService.GetOrDefault(Setting.SkipCacheCheck, false);
            signInSilently = SettingService.GetOrDefault(Setting.SignInSilently, false);
            bypassCharactersLimit = SettingService.GetOrDefault(Setting.BypassCharactersLimit, false);

            //version
            Version v = Assembly.GetExecutingAssembly().GetName().Version!;
            VersionString = $"DGP.Genshin - version {v.Major}.{v.Minor}.{v.Build} Build {v.Revision}";
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
                case Setting.BypassCharactersLimit:
                    BypassCharactersLimit = value is not null && (bool)value;
                    break;
                default:
                    break;
            }
        }

        private string versionString;
        private AutoRun autoRun = new();
        private NamedValue<ApplicationTheme?> selectedTheme;
        private IAsyncRelayCommand checkUpdateCommand;

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
                SetAppTheme();
            }
        }
        internal void SetAppTheme()
        {
            ThemeManager.Current.ApplicationTheme = SettingService.GetOrDefault(Setting.AppTheme, null, Setting.ApplicationThemeConverter);
        }
        public IAsyncRelayCommand CheckUpdateCommand
        {
            get => checkUpdateCommand;
            [MemberNotNull(nameof(checkUpdateCommand))]
            set => checkUpdateCommand = value;
        }
        public ISettingService SettingService => settingService;
    }
}