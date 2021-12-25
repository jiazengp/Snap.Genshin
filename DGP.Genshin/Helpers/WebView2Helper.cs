using DGP.Genshin.Common.Core.Logging;
using Microsoft.Win32;

namespace DGP.Genshin.Helpers
{
    /// <summary>
    /// 检测 WebView2运行时 是否存在
    /// </summary>
    internal class WebView2Helper
    {
        private const string pvKey = "pv";
        private const string Path = @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}";

        /// <summary>
        /// 检测WebView
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                RegistryKey currentUser = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey? subKey = currentUser.OpenSubKey(Path);
                Logger.LogStatic(subKey?.GetValue(pvKey));
                return subKey?.GetValue(pvKey) is not null;
            }
        }
    }
}
