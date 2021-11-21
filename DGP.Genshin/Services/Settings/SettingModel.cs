using DGP.Genshin.Common.Data.Behavior;
using DGP.Genshin.Common.Extensions.System;
using System.Diagnostics.CodeAnalysis;

namespace DGP.Genshin.Services.Settings
{
    /// <summary>
    /// 为需要及时响应的设置项提供 <see cref="Observable"/> 模型支持
    /// 仅供 <see cref="Pages.SettingsPage"/> 使用
    /// </summary>
    public class SettingModel : Observable
    {
        /// <summary>
        /// 当以编程形式改变设置时触发
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void SettingChanged(string key, object? value)
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
                    SignInSilently = value is not null && (bool) value;
                    break;
                default:
                    break;
            }
        }

        private bool showFullUID;
        private bool autoDailySignInOnLaunch;
        private bool skipCacheCheck;
        private bool signInSilently;

        public bool ShowFullUID
        {
            get => showFullUID; set
            {
                SettingService.Instance.SetValueInternal(Setting.ShowFullUID, value);
                Set(ref showFullUID, value);
            }
        }

        public bool AutoDailySignInOnLaunch
        {
            get => autoDailySignInOnLaunch; set
            {
                SettingService.Instance.SetValueInternal(Setting.AutoDailySignInOnLaunch, value);
                Set(ref autoDailySignInOnLaunch, value);
            }
        }

        public bool SkipCacheCheck
        {
            get => skipCacheCheck; set
            {
                SettingService.Instance.SetValueInternal(Setting.SkipCacheCheck, value);
                Set(ref skipCacheCheck, value);
            }
        }

        public bool SignInSilently
        {
            get => signInSilently; set
            {
                SettingService.Instance.SetValueInternal(Setting.SignInSilently, value);
                Set(ref signInSilently, value);
            }
        }

        private void Initialize()
        {
            SettingService service = SettingService.Instance;

            showFullUID = service.GetOrDefault(Setting.ShowFullUID, false);
            autoDailySignInOnLaunch = service.GetOrDefault(Setting.AutoDailySignInOnLaunch, false);
            skipCacheCheck = service.GetOrDefault(Setting.SkipCacheCheck, false);
            signInSilently= service.GetOrDefault(Setting.SignInSilently, false);
        }

        #region 单例
        private static volatile SettingModel? instance;
        [SuppressMessage("", "IDE0044")]
        private static object _locker = new();
        private SettingModel()
        {
            Initialize();
            SettingService.Instance.SettingChanged += SettingChanged;
        }
        public static SettingModel Instance
        {
            get
            {
                if (instance is null)
                {
                    lock (_locker)
                    {
                        instance ??= new();
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
