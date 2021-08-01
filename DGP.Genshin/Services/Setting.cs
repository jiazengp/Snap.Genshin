using System;

namespace DGP.Genshin.Services
{
    public static class Setting
    {
        [Obsolete] public const string ShowUnreleasedData = "ShowUnreleasedCharacter";
        [Obsolete] public const string PresentTravelerElementType = "PresentTravelerElementType";
        public const string AppTheme = "AppTheme";
        public const string IsDevMode = "IsDevMode";
        public const string Uid = "Uid";
        public static T EnumConverter<T>(object n) => (T)Enum.Parse(typeof(T), n.ToString());
    }
}
