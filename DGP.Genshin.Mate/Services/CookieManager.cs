using DGP.Genshin.Common.Data.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace DGP.Genshin.Mate.Services
{
    /// <summary>
    /// 全局Cookie管理器
    /// </summary>
    public static class CookieManager
    {
        private const string CookieListFile = "cookielist.dat";

        internal static string Base64Decode(Encoding encoding, string input)
        {
            byte[] bytes = Convert.FromBase64String(input);
            return encoding.GetString(bytes);
        }

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
                Cookies = new CookiePool(base64Cookies.Select(b => Base64Decode(Encoding.UTF8, b)));
            }
            else
            {
                Cookies = new();
                File.Create(CookieListFile).Dispose();
            }
        }
    }
}
