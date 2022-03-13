using DGP.Genshin.Service.Abstraction.Setting;

namespace DGP.Genshin.Service.Abstraction.Launching
{
    /// <summary>
    /// 封装启动参数
    /// </summary>
    public class LaunchOption
    {
        public bool IsBorderless { get; set; }
        public bool IsFullScreen { get; set; }
        public bool UnlockFPS { get; set; }
        public int TargetFPS { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }

        /// <summary>
        /// 使用当前的设置项创建启动参数
        /// </summary>
        /// <returns></returns>
        public static LaunchOption FromCurrentSettings()
        {
            return new()
            {
                IsBorderless = Setting2.IsBorderless.Get(),
                IsFullScreen = Setting2.IsFullScreen.Get(),
                UnlockFPS = App.IsElevated && Setting2.UnlockFPS.Get(),
                TargetFPS = (int)Setting2.TargetFPS.Get(),
                ScreenWidth = (int)Setting2.ScreenWidth.Get(),
                ScreenHeight = (int)Setting2.ScreenHeight.Get()
            };
        }
    }
}
