using System;
using System.IO;
using System.Threading.Tasks;

namespace DGP.Genshin.Models.MiHoYo
{
    public static class CookieManager
    {
        private static readonly string CookieFile = "cookie.dat";
        static CookieManager()
        {
            if (File.Exists(CookieFile))
            {
                Cookie = File.ReadAllText(CookieFile);
            }
            else
            {
                File.Create(CookieFile).Dispose();
            }
        }
        private static string Cookie { get; set; }

        public static async Task<string> GetCookieAsync()
        {
            if (String.IsNullOrEmpty(Cookie))
            {
                await SetCookieAsync();
            }
            return Cookie;
        }

        public static async Task SetCookieAsync()
        {
            Cookie = await System.Windows.Application.Current.Dispatcher.Invoke(
                async () => await new CookieDialog().GetInputCookieAsync());
            CookieChanged?.Invoke();
            File.WriteAllText(CookieFile, Cookie);
        }

        public static event Action CookieChanged;
    }
}