using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Helpers;
using DGP.Genshin.Services.Abstratcions;
using DGP.Genshin.Services.Settings;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DGP.Genshin.ViewModels
{
    /// <summary>
    /// 为需要及时响应的设置项提供 <see cref="Observable"/> 模型支持
    /// 仅供 <see cref="Pages.SettingsPage"/> 使用
    /// </summary>
    [ViewModel(ViewModelType.Singleton)]
    public class SettingViewModel : ObservableObject
    {
        private readonly ISettingService settingService;

        public SettingViewModel(ISettingService settingService)
        {
            this.settingService = settingService;
            Initialize();
            settingService.SettingChanged += SettingServiceSettingChanged;
        }

        /// <summary>
        /// 当以编程形式改变设置时触发
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void SettingServiceSettingChanged(string key, object? value)
        {
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
                settingService.SetValueNoNotify(Setting.ShowFullUID, value);
                SetProperty(ref showFullUID, value);
            }
        }

        public bool AutoDailySignInOnLaunch
        {
            get => autoDailySignInOnLaunch; set
            {
                settingService.SetValueNoNotify(Setting.AutoDailySignInOnLaunch, value);
                SetProperty(ref autoDailySignInOnLaunch, value);
            }
        }

        public bool SkipCacheCheck
        {
            get => skipCacheCheck; set
            {
                settingService.SetValueNoNotify(Setting.SkipCacheCheck, value);
                SetProperty(ref skipCacheCheck, value);
            }
        }

        public bool SignInSilently
        {
            get => signInSilently; set
            {
                settingService.SetValueNoNotify(Setting.SignInSilently, value);
                SetProperty(ref signInSilently, value);
            }
        }

        public bool BypassCharactersLimit
        {
            get => bypassCharactersLimit; set
            {
                settingService.SetValueNoNotify(Setting.BypassCharactersLimit, value);
                SetProperty(ref bypassCharactersLimit, value);
            }
        }
        #endregion

        [MemberNotNull("versionString")]
        private void Initialize()
        {
            showFullUID = settingService.GetOrDefault(Setting.ShowFullUID, false);
            autoDailySignInOnLaunch = settingService.GetOrDefault(Setting.AutoDailySignInOnLaunch, false);
            skipCacheCheck = settingService.GetOrDefault(Setting.SkipCacheCheck, false);
            signInSilently = settingService.GetOrDefault(Setting.SignInSilently, false);
            bypassCharactersLimit = settingService.GetOrDefault(Setting.BypassCharactersLimit, false);

            //version
            Version v = Assembly.GetExecutingAssembly().GetName().Version!;
            versionString = $"DGP.Genshin - version {v.Major}.{v.Minor}.{v.Build} Build {v.Revision}";
        }

        private string versionString;
        private AutoRun autoRun = new();

        public string VersionString { get => versionString; set => SetProperty(ref versionString, value); }

        public AutoRun AutoRun { get => autoRun; set => autoRun = value; }
    }
}
