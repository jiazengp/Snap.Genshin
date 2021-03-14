using DGP.Genshin.Services;
using DGP.Snap.Framework.Core.LifeCycle;
using ModernWpf;
using System;

namespace DGP.Genshin.Helpers
{
    internal class ThemeHelper
    {
        internal static void SetAppTheme()
        {
            static ApplicationTheme? converter(object n) { if (n == null) { return null; } return (ApplicationTheme)Enum.Parse(typeof(ApplicationTheme), n.ToString()); }
            ThemeManager.Current.ApplicationTheme = LifeCycle.InstanceOf<SettingService>().GetOrDefault(Setting.AppTheme, null, converter);
        }
    }
}
