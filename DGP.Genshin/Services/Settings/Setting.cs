using ModernWpf;
using System;

namespace DGP.Genshin.Services.Settings
{
    public static class Setting
    {
        #region Obsolete
        [Obsolete] public const string ShowUnreleasedData = "ShowUnreleasedCharacter";
        [Obsolete] public const string PresentTravelerElementType = "PresentTravelerElementType";
        [Obsolete] public const string Uid = "Uid";
        #endregion

        public const string AppTheme = "AppTheme";
        public const string IsDevMode = "IsDevMode";
        public const string ShowFullUID = "ShowFullUID";
        public const string AutoDailySignInOnLaunch = "AutoDailySignInOnLaunch";
        public const string LauncherPath = "LauncherPath";

        public static ApplicationTheme? ApplicationThemeConverter(object n) =>
            n == null ? null : (ApplicationTheme)Enum.ToObject(typeof(ApplicationTheme), n);
    }
}
