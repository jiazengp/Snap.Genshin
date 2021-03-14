using System;

namespace DGP.Genshin.Services
{
    public class Setting
    {
        public const string ShowUnreleasedData = "ShowUnreleasedCharacter";
        public const string PresentTravelerElementType = "PresentTravelerElementType";
        public const string AppTheme = "AppTheme";
        public static T EnumConverter<T>(object n) => (T)Enum.Parse(typeof(T), n.ToString());
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SettingAttribute : Attribute
    {

    }
}
