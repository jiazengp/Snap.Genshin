using DGP.Genshin.Common.Core.Logging;
using DGP.Genshin.Common.Extensions.System;
using Microsoft.Win32;

namespace DGP.Genshin.Helpers
{
    /// <summary>
    /// 检测 WebView2运行时 是否存在
    /// </summary>
    internal class WebView2Helper
    {
        private const string value = "pv";
        private const string path = @"SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}";

        /// <summary>
        /// 检测WebView
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                RegistryKey currentUser = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey? subKey = currentUser.OpenSubKey(path);
                Logger.LogStatic(subKey?.GetValue(value));
                return subKey?.GetValue(value) is not null;
            }
        }
    }
}
