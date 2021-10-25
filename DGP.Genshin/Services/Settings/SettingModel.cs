using DGP.Genshin.Common.Data.Behavior;
using DGP.Genshin.Common.Extensions.System;

namespace DGP.Genshin.Services.Settings
{
    /// <summary>
    /// 为需要及时响应的设置项提供 <see cref="Observable"/> 模型支持
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
            this.Log($"receive setting changed event {key}:{value}");
            switch (key)
            {
                case Setting.ShowFullUID:
                    ShowFullUID = value is not null && (bool)value;
                    break;
                case Setting.AutoDailySignInOnLaunch:
                    AutoDailySignInOnLaunch = value is not null && (bool)value;
                    break;
                default:
                    break;
            }
        }

        private bool showFullUID;
        private bool autoDailySignInOnLaunch;

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

        #region 单例
        private static SettingModel? instance;
        private static readonly object _lock = new();
        private SettingModel()
        {
            Initialize();
            SettingService.Instance.SettingChanged += SettingChanged;
        }

        private void Initialize()
        {
            SettingService service = SettingService.Instance;

            showFullUID = service.GetOrDefault(Setting.ShowFullUID, false);
            autoDailySignInOnLaunch = service.GetOrDefault(Setting.AutoDailySignInOnLaunch, false);
        }

        public static SettingModel Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new SettingModel();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
