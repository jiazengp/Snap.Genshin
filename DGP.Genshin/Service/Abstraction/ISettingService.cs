using DGP.Genshin.DataModel.DailyNote;
using ModernWpf;
using Snap.Data.Json;
using System;

namespace DGP.Genshin.Service.Abstraction
{
    /// <summary>
    /// 设置服务
    /// 否则会影响已有的设置值
    /// </summary>
    public interface ISettingService
    {
        /// <summary>
        /// 使用定义获取设置值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definition"></param>
        /// <returns></returns>
        T Get<T>(SettingDefinition<T> definition);

        /// <summary>
        /// 初始化设置服务，加载设置数据
        /// </summary>
        void Initialize();

        /// <summary>
        /// 使用定义设置设置值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="definition"></param>
        /// <param name="value"></param>
        /// <param name="notify"></param>
        /// <param name="log"></param>
        void Set<T>(SettingDefinition<T> definition, object? value, bool notify = true, bool log = false);

        /// <summary>
        /// 卸载设置数据
        /// </summary>
        void UnInitialize();
    }

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

        public static ApplicationTheme? ApplicationThemeConverter(object? n)
        {
            return n is null ? null : (ApplicationTheme)Enum.ToObject(typeof(ApplicationTheme), n);
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

    /// <summary>
    /// 设置入口定义
    /// </summary>
    /// <typeparam name="T">设置项定义</typeparam>
    public class SettingDefinition<T>
    {
        public static readonly ISettingService settingService;
        static SettingDefinition()
        {
            settingService = App.AutoWired<ISettingService>();
        }

        public SettingDefinition(string name, T defaultValue, Func<object, T>? converter = null)
        {
            Name = name;
            DefaultValue = defaultValue;
            Converter = converter;
        }

        public string Name { get; }
        public T DefaultValue { get; }
        public Func<object, T>? Converter { get; }

        public T Get()
        {
            return settingService.Get(this);
        }

        public void Set(object? value, bool notify = true, bool log = false)
        {
            settingService.Set(this, value, notify, log);
        }

        /// <summary>
        /// 提供单参数重载以便 <see cref="Snap.Core.Mvvm.ObservableObject2"/> 的通知方法调用
        /// </summary>
        /// <param name="value"></param>
        public void Set(T value)
        {
            Set(value, true, false);
        }
    }
}
