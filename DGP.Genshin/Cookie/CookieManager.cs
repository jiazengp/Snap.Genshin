using DGP.Genshin.Common.Core.Logging;
using DGP.Genshin.Common.Data.Json;
using DGP.Genshin.Common.Exceptions;
using DGP.Genshin.Common.Extensions.System;
using DGP.Genshin.Helpers;
using System;
using System.Collections.Generic;
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

        private readonly static object _savingCookie = new();
        private readonly static object _savingCookies = new();

        private static string? currentCookie;

        /// <summary>
        /// 备选Cookie池
        /// </summary>
        public static CookiePool Cookies { get; set; }

        static CookieManager()
        {
            //load cookies
            try
            {
                CookiePool base64Cookies = Json.FromFile<CookiePool>(CookieListFile) ?? new();
                Cookies = new CookiePool(base64Cookies.Select(b => TokenHelper.Base64Decode(Encoding.UTF8, b)));
            }
            catch (Exception ex)
            {
                ex.Log($"{ex.Message}");
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
                Logger.LogStatic("无可用的Cookie");
                File.Create(CookieFile).Dispose();
            }

            if (currentCookie is not null)
            {
                Cookies.AddOrIgnore(currentCookie);
            }
        }

        /// <summary>
        /// 当前使用的Cookie，由<see cref="CookieManager"/> 保证不为 <see cref="null"/>
        /// </summary>
        public static string CurrentCookie
        {
            get => currentCookie ?? throw new SnapGenshinInternalException("Cookie 不应为 null");
            private set
            {
                if (currentCookie == value)
                {
                    return;
                }
                currentCookie = value;
                if (!Cookies.Contains(currentCookie))
                {
                    Cookies.Add(currentCookie);
                }
                Logger.LogStatic("current cookie has changed");
                CookieChanged?.Invoke();
            }
        }

        /// <summary>
        /// 用于在初始化时判断Cookie是否可用
        /// </summary>
        public static bool IsCookieAvailable =>
            !string.IsNullOrEmpty(currentCookie);

        /// <summary>
        /// 设置新的Cookie
        /// </summary>
        public static async Task SetCookieAsync()
        {
            string cookie = await App.Current.Dispatcher.InvokeAsync(new CookieDialog().GetInputCookieAsync).Task.Unwrap();
            //prevent user input unexpected invalid cookie
            if (cookie != string.Empty)
            {
                CurrentCookie = cookie;
            }
            lock (_savingCookie)
            {
                File.WriteAllText(CookieFile, CurrentCookie);
            }
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
            lock (_savingCookies)
            {
                List<string> encodedCookies = (Cookies as List<string>)
                .Select(c => TokenHelper.Base64Encode(Encoding.UTF8, c))
                //.ToList() fix json parse error
                .ToList();
                Json.ToFile(CookieListFile, encodedCookies);
            }
        }

        /// <summary>
        /// Cookie改变时触发
        /// </summary>
        public static event Action? CookieChanged;
    }
}