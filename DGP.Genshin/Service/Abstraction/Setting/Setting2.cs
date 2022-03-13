using DGP.Genshin.DataModel.DailyNote;
using DGP.Genshin.Service.Abstraction.Updating;
using ModernWpf;
using Snap.Data.Json;
using System;

namespace DGP.Genshin.Service.Abstraction.Setting
{
    /// <summary>
    /// 全新的设置接口
    /// </summary>
    public static class Setting2
    {
        //APP setting
        public static readonly SettingDefinition<ApplicationTheme?> AppTheme = new("AppTheme", null, ApplicationThemeConverter);
        public static readonly SettingDefinition<Version?> AppVersion = new("AppVersion", null, VersionConverter);
        //sign in
        public static readonly SettingDefinition<DateTime?> LastAutoSignInTime = new("LastAutoSignInTime", DateTime.Today.AddDays(-1), NullableDataTimeConverter);
        public static readonly SettingDefinition<bool> AutoDailySignInOnLaunch = new("AutoDailySignInOnLaunch", false);
        public static readonly SettingDefinition<bool> SignInSilently = new("SignInSilently", false);
        //launch
        public static readonly SettingDefinition<string?> LauncherPath = new("LauncherPath", null);
        public static readonly SettingDefinition<bool> IsFullScreen = new("IsFullScreenLaunch", true);
        public static readonly SettingDefinition<bool> IsBorderless = new("IsBorderlessLaunch", true);
        public static readonly SettingDefinition<bool> UnlockFPS = new("FPSUnlockingEnabled", false);
        public static readonly SettingDefinition<double> TargetFPS = new("FPSUnlockingTarget", 60D);
        public static readonly SettingDefinition<long> ScreenWidth = new("LaunchScreenWidth", 1920);
        public static readonly SettingDefinition<long> ScreenHeight = new("LaunchScreenHeight", 1080);
        //boot up
        public static readonly SettingDefinition<bool> SkipCacheCheck = new("SkipCacheCheck", false);
        //update
        public static readonly SettingDefinition<UpdateAPI> UpdateAPI = new("UpdateChannel", Updating.UpdateAPI.PatchAPI, UpdateAPIConverter);
        public static readonly SettingDefinition<bool> UpdateUseFastGit = new("UpdateUseFastGit", false);
        //gacha statistic
        public static readonly SettingDefinition<bool> IsBannerWithNoItemVisible = new("IsBannerWithNoItemVisible", true);
        //resin
        public static readonly SettingDefinition<double> ResinRefreshMinutes = new("ResinRefreshMinutes", 8D);
        public static readonly SettingDefinition<DailyNoteNotifyConfiguration?> DailyNoteNotifyConfiguration = new("DailyNoteNotifyConfiguration", null, ComplexConverter<DailyNoteNotifyConfiguration>);
        //taskbar
        public static readonly SettingDefinition<bool> IsTaskBarIconEnabled = new("IsTaskBarIconEnabled", true);
        public static readonly SettingDefinition<bool> IsTaskBarIconHintDisplay = new("IsTaskBarIconHintDisplay", true);
        //main window
        public static readonly SettingDefinition<bool> CloseMainWindowAfterInitializaion = new("CloseMainWindowAfterInitializaion", false);
        public static readonly SettingDefinition<double> MainWindowWidth = new("MainWindowWidth", 0D);
        public static readonly SettingDefinition<double> MainWindowHeight = new("MainWindowHeight", 0D);
        public static readonly SettingDefinition<bool> IsNavigationViewPaneOpen = new("IsNavigationViewPaneOpen", true);
        //background
        public static readonly SettingDefinition<double> BackgroundOpacity = new("BackgroundOpacity", 0.4);
        public static readonly SettingDefinition<bool> IsBackgroundOpacityAdaptive = new("IsBackgroundOpacityAdaptive", false);

        public static ApplicationTheme? ApplicationThemeConverter(object? obj)
        {
            return obj is null ? null : (ApplicationTheme)Enum.ToObject(typeof(ApplicationTheme), obj);
        }
        public static UpdateAPI UpdateAPIConverter(object obj)
        {
            return (UpdateAPI)Enum.ToObject(typeof(ApplicationTheme), obj);
        }
        public static Version? VersionConverter(object? obj)
        {
            return obj is string str ? Version.Parse(str) : null;
        }
        public static DateTime? NullableDataTimeConverter(object? str)
        {
            return str is not null ? DateTime.Parse((string)str) : null;
        }
        public static T? ComplexConverter<T>(object? value) where T : class
        {
            return value is null ? null : Json.ToObject<T>(value.ToString()!);
        }
    }
}
