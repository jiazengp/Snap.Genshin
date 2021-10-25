using DGP.Snap.Framework.Core.Logging;
using DGP.Snap.Framework.Extensions.System.Windows.Threading;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DGP.Genshin.Cookie
{
    /// <summary>
    /// 全局Cookie管理器
    /// </summary>
    public class CookieManager
    {
        private static readonly string CookieFile = "cookie.dat";
        private static string? cookie;

        static CookieManager()
        {
            if (File.Exists(CookieFile))
            {
                Cookie = File.ReadAllText(CookieFile);
            }
            else
            {
                Logger.LogStatic(typeof(CookieManager), "无可用的Cookie");
                File.Create(CookieFile).Dispose();
            }
        }

        public static string Cookie
        {
            get
            {
                return cookie ?? throw new InvalidOperationException("Cookie 不应为 null");
            }
            private set
            {
                cookie = value;
                CookieRefreshed?.Invoke();
            }
        }

        /// <summary>
        /// 用于在初始化时判断Cookie是否可用
        /// </summary>
        public static bool IsCookieAvailable =>
            !string.IsNullOrEmpty(Cookie);

        /// <summary>
        /// 设置新的Cookie
        /// </summary>
        /// <returns></returns>
        public static async Task SetCookieAsync()
        {
            Cookie = await System.Windows.Application.Current.Invoke(new CookieDialog().GetInputCookieAsync);
            File.WriteAllText(CookieFile, Cookie);
        }

        /// <summary>
        /// Cookie改变时触发
        /// </summary>
        public static event Action? CookieRefreshed;
    }
}