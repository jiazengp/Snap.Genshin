using ModernWpf;
using System;

namespace DGP.Genshin.Services.Settings
{
    public static class Setting
    {
        [Obsolete] public const string ShowUnreleasedData = "ShowUnreleasedCharacter";
        [Obsolete] public const string PresentTravelerElementType = "PresentTravelerElementType";
        public const string AppTheme = "AppTheme";
        public const string IsDevMode = "IsDevMode";
        public const string Uid = "Uid";

        public static ApplicationTheme? ApplicationThemeConverter(object n) =>
            n == null ? null : (ApplicationTheme)Enum.ToObject(typeof(ApplicationTheme), n);
    }
}
