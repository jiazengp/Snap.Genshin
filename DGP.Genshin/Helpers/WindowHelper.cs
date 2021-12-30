using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

using static DGP.Genshin.Common.NativeMethods.User32;

namespace DGP.Genshin.Helpers
{
    public static class WindowHelper
    {
        /// <summary>
        /// 使窗体置于桌面底端
        /// </summary>
        /// <param name="window"></param>
        public static void SetInDesktop(this Window window)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            //notify windows to create a WorkerW
            SendMessageTimeout(FindWindow("Progman", ""), 1324u, new UIntPtr(0u), IntPtr.Zero, SendMessageTimeoutFlags.SMTO_NORMAL, 1000u, out var _);
            SetParent(handle, FindWorkerWPtr());
        }

        private static IntPtr FindWorkerWPtr()
        {
            IntPtr workerw = IntPtr.Zero;
            IntPtr def = IntPtr.Zero;
            IntPtr result = FindWindow("Progman", null);
            _ = IntPtr.Zero;
            EnumWindows(delegate (IntPtr handle, IntPtr param)
            {
                if ((def = FindWindowEx(handle, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero)) != IntPtr.Zero)
                {
                    workerw = FindWindowEx(IntPtr.Zero, handle, "WorkerW", IntPtr.Zero);
                    Console.Write("workerw:" + workerw + "\n");
                    ShowWindow(workerw, 0);
                }
                return true;
            }, IntPtr.Zero);
            return result;
        }

        private static readonly uint acrylicBackgroundColor = 0x808080; /* BGR color format */

        /// <summary>
        /// 启用亚克力
        /// </summary>
        /// <param name="window"></param>
        internal static void EnableAcrylic(this Window window)
        {
            AccentPolicy accent = new ()
            {
                AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                GradientColor = (0 << 24) | (acrylicBackgroundColor & 0xFFFFFF)
            };

            int accentStructSize = Marshal.SizeOf(accent);

            IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            WindowCompositionAttributeData data = new()
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            _ = SetWindowCompositionAttribute(new WindowInteropHelper(window).Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }
    }
}
