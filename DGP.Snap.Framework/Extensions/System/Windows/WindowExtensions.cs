using System;
using System.Windows;
using System.Windows.Interop;

namespace DGP.Snap.Framework.Extensions.System.Windows
{
    public static class WindowExtensions
    {
        /// <summary>
        /// 获取窗体的句柄
        /// </summary>
        /// <param name="window"></param>
        /// <returns>整型窗体句柄指针</returns>
        public static IntPtr GetHandle(this Window window)
        {
            return new WindowInteropHelper(window).Handle;
        }
    }
}
