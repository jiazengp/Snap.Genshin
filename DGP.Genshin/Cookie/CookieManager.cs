using DGP.Genshin.Common;
using DGP.Genshin.Common.Core.Logging;
using DGP.Genshin.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGP.Genshin.Cookie
{
    /// <summary>
    /// 全局Cookie管理器
    /// </summary>
    public static class CookieManager
    {
        private const string CookieFile = "cookie.dat";
        private const string CookieListFile = "cookielist.dat";

        private static string? cookie;

        /// <summary>
        /// 备选Cookie池
        /// </summary>
        public static CookiePool Cookies { get; set; }

        static CookieManager()
        {
            //load cookies
            if (File.Exists(CookieListFile))
            {
                CookiePool base64Cookies = Json.FromFile<CookiePool>(CookieListFile) ?? new();
                Cookies = new CookiePool(base64Cookies.Select(b => TokenHelper.Base64Decode(Encoding.UTF8, b)));
            }
            else
            {
                Cookies = new();
                File.Create(CookieListFile).Dispose();
            }
            //load cookie
            if (File.Exists(CookieFile))
            {
                CurrentCookie = File.ReadAllText(CookieFile);
            }
            else
            {
                Logger.LogStatic(typeof(CookieManager), "无可用的Cookie");
                File.Create(CookieFile).Dispose();
            }
            if (cookie is not null)
            {
                Cookies.AddOrIgnore(cookie);
            }
        }

        /// <summary>
        /// 当前使用的Cookie，由<see cref="CookieManager"/> 保证不为 <see cref="null"/>
        /// </summary>
        public static string CurrentCookie
        {
            get => cookie ?? throw new InvalidOperationException("Cookie 不应为 null");
            private set
            {
                if (cookie == value)
                {
                    return;
                }
                cookie = value;
                if (!Cookies.Contains(cookie))
                {
                    Cookies.Add(cookie);
                }
                Logger.LogStatic(typeof(CookieManager), "current cookie has changed");
                CookieChanged?.Invoke();
            }
        }

        /// <summary>
        /// 用于在初始化时判断Cookie是否可用
        /// </summary>
        public static bool IsCookieAvailable =>
            !string.IsNullOrEmpty(cookie);

        /// <summary>
        /// 设置新的Cookie
        /// </summary>
        public static async Task SetCookieAsync()
        {
            CurrentCookie = await App.Current.Dispatcher.InvokeAsync(new CookieDialog().GetInputCookieAsync).Task.Unwrap();
            File.WriteAllText(CookieFile, CurrentCookie);
        }

        internal static void ChangeOrIgnoreCurrentCookie(string? cookie)
        {
            if (cookie is null)
            {
                return;
            }
            if (CurrentCookie != cookie)
            {
                CurrentCookie = cookie;
            }
        }

        public static async Task AddNewCookieToPoolAsync()
        {
            string newCookie = await App.Current.Dispatcher.InvokeAsync(new CookieDialog().GetInputCookieAsync).Task.Unwrap();
            if (!string.IsNullOrEmpty(newCookie))
            {
                Cookies.AddOrIgnore(newCookie);
            }
        }

        public static void SaveCookies()
        {
            Json.ToFile(CookieListFile, Cookies.Select(c => TokenHelper.Base64Encode(Encoding.UTF8, c)));
        }

        /// <summary>
        /// Cookie改变时触发
        /// </summary>
        public static event Action? CookieChanged;
    }
}