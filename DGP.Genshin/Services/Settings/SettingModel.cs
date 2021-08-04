using DGP.Snap.Framework.Data.Behavior;

namespace DGP.Genshin.Services.Settings
{
    public class SettingModel : Observable
    {
        private bool isDevMode = SettingService.Instance.GetOrDefault(Setting.IsDevMode, false);
        public bool IsDevMode
        {
            get => this.isDevMode;
            set => this.Set(ref this.isDevMode, value);
        }

        #region 单例
        private static SettingModel instance;

        private static readonly object _lock = new();
        private SettingModel()
        {
            SettingService.SettingChanged += (s, o) =>
            {
                switch (s)
                {
                    case Setting.IsDevMode:
                        this.IsDevMode = o != null && (bool)o;
                        break;
                }
            };
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
