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
            if (string.IsNullOrEmpty(Cookie))
            {
                string cookie = await new CookieDialog().GetInputCookieAsync();
                File.WriteAllText(CookieFile, cookie);
                Cookie = cookie;
            }
            return Cookie;
        }

        public static void ResetCookie()
        {
            Cookie = null;
        }
    }
}