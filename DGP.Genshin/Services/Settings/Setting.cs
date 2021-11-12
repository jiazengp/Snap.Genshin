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
        public const string LauncherPath = "LauncherPath";
        public const string IsBorderless = "IsBorderlessLaunch";
        public const string LastAutoSignInTime = "LastAutoSignInTime";
        public const string AppVersion = "AppVersion";

        public static ApplicationTheme? ApplicationThemeConverter(object? n)
        {
            return n is null ? null : (ApplicationTheme)Enum.ToObject(typeof(ApplicationTheme), n);
        }

        public static Version? VersionConverter(object? obj)
        {
            return obj is string str ? Version.Parse(str) : null;
        }
        
    }
}
