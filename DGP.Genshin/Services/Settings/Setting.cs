using ModernWpf;
using System;

namespace DGP.Genshin.Services.Settings
{
    public static class Setting
    {
        public const string AppTheme = "AppTheme";
        public const string IsDevMode = "IsDevMode";
        public const string ShowFullUID = "ShowFullUID";
        public const string AutoDailySignInOnLaunch = "AutoDailySignInOnLaunch";
        public const string SignInSilently = "SignInSilently";
        public const string LauncherPath = "LauncherPath";
        public const string IsFullScreen = "IsFullScreenLaunch";
        public const string IsBorderless = "IsBorderlessLaunch";
        public const string LastAutoSignInTime = "LastAutoSignInTime";
        public const string AppVersion = "AppVersion";
        public const string SkipCacheCheck = "SkipCacheCheck";
        public const string BypassCharactersLimit = "BypassCharactersLimit";

        public static ApplicationTheme? ApplicationThemeConverter(object? n)
        {
            return n is null ? null : (ApplicationTheme)Enum.ToObject(typeof(ApplicationTheme), n);
        }

        public static Version? VersionConverter(object? obj)
        {
            return obj is string str ? Version.Parse(str) : null;
        }

    }

    /// <summary>
    /// 设置项改变委托
    /// </summary>
    /// <param name="key">设置项名称</param>
    /// <param name="value">项的值</param>
    public delegate void SettingChangedHandler(string key, object? value);
}
