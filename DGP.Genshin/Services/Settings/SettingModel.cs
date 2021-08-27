using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Extensions.System;

namespace DGP.Genshin.Services.Settings
{
    /// <summary>
    /// 为需要及时响应的设置项提供模型支持
    /// </summary>
    public class SettingModel : Observable
    {
        private void SettingChanged(string key, object value)
        {
            this.Log($"receive setting changed event {key}:{value}");
            switch (key)
            {
                case Setting.ShowFullUID:
                    this.ShowFullUID = (bool)value;
                    break;
                default:
                    break;
            }
        }

        public bool ShowFullUID
        {
            get => this.showFullUID; set
            {
                SettingService.Instance.SetValueInternal(Setting.ShowFullUID, value);
                Set(ref this.showFullUID, value);
            }
        }

        #region 单例
        private static SettingModel instance;
        private bool showFullUID;
        private static readonly object _lock = new();
        private SettingModel()
        {
            Initialize();
            SettingService.Instance.SettingChanged += SettingChanged;
        }

        private void Initialize() => this.showFullUID = SettingService.Instance.GetOrDefault(Setting.ShowFullUID, false);

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
