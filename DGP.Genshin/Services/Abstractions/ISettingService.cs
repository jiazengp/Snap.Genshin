using DGP.Genshin.Common.Core.DependencyInjection;
using DGP.Genshin.Messages;
using ModernWpf;
using System;

namespace DGP.Genshin.Services.Abstratcions
{
    /// <summary>
    /// 设置服务
    /// 使用此服务时需要注意储存值的键名称不能与 <see cref="Setting"/> 内的相同
    /// 否则会影响已有的设置值
    /// </summary>
    [Send(typeof(SettingChangedMessage))]
    public interface ISettingService
    {
        /// <summary>
        /// 设置索引器
        /// </summary>
        /// <param name="key">设置键</param>
        /// <returns>对应的设置值</returns>
        object? this[string key] { set; }

        /// <summary>
        /// 读取设置值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认填充返回值</param>
        /// <returns>设置值</returns>
        T? GetOrDefault<T>(string key, T? defaultValue);

        /// <summary>
        /// 读取设置值
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认填充返回值</param>
        /// <param name="converter">类型转换器</param>
        /// <returns>设置值</returns>
        T GetOrDefault<T>(string key, T defaultValue, Func<object, T> converter);

        /// <summary>
        /// 初始化设置服务，加载设置数据
        /// </summary>
        void Initialize();

        /// <summary>
        /// 以不发送消息的形式更新设置值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        void SetValueNoNotify(string key, object value);

        /// <summary>
        /// 卸载设置数据
        /// </summary>
        void UnInitialize();
    }

    /// <summary>
    /// 用户设置索引
    /// </summary>
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
        public const string UpdateUseFastGit = "UpdateUseFastGit";
        public const string ResinRefreshMinutes = "ResinRefreshMinutes";

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
