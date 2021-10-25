using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DGP.Genshin.Common.NativeMethods
{
    public class WinInet
    {
        public const int COOKIE_HTTP_ONLY = 0x00002000;
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetGetCookieEx(
            string url, string cookieName, StringBuilder cookieData,
            ref uint cookieSize, int flags, IntPtr reversed);
    }
}
