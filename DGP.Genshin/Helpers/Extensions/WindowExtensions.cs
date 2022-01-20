using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

using static Snap.Win32.NativeMethod.User32;

namespace DGP.Genshin.Helpers.Extensions
{
    public static class WindowExtensions
    {
        #region Bottom Most Window
        /// <summary>
        /// 使窗体置于桌面底端
        /// </summary>
        /// <param name="window"></param>
        public static void SetInDesktop(this Window window)
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;
            //notify windows to create a WorkerW
            IntPtr hProgManWnd = FindWindow("Progman", "Program Manager");
            SendMessageTimeout(hProgManWnd, 1324u, new UIntPtr(0u), IntPtr.Zero, SendMessageTimeoutFlags.SMTO_NORMAL, 1000u, out UIntPtr _);
            ConfigureWorkerW();
            SetParent(hWnd, hProgManWnd);
        }

        private static void ConfigureWorkerW()
        {
            IntPtr hWorkerWnd = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "WorkerW", IntPtr.Zero);

            while (hWorkerWnd != IntPtr.Zero)
            {
                uint dwStyle = GetWindowLong(hWorkerWnd, GWL_STYLE);
                if ((dwStyle & WS_VISIBLE) != 0)
                {
                    IntPtr hdefWnd = FindWindowEx(hWorkerWnd, IntPtr.Zero, "SHELLDLL_DefView", IntPtr.Zero);
                    if (hdefWnd != IntPtr.Zero)
                    {
                        hWorkerWnd = FindWindowEx(IntPtr.Zero, hWorkerWnd, "WorkerW", IntPtr.Zero);
                        break;
                    }
                }
                hWorkerWnd = FindWindowEx(IntPtr.Zero, hWorkerWnd, "WorkerW", IntPtr.Zero);
            }

            _ = ShowWindow(hWorkerWnd, SW_HIDE);
        }
        #endregion

        #region Acrylic
        private static readonly uint acrylicBackgroundColor = 0x808080; /* BGR color format */

        /// <summary>
        /// 启用亚克力
        /// </summary>
        /// <param name="window"></param>
        internal static void EnableAcrylic(this Window window)
        {
            AccentPolicy accent = new()
            {
                AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                GradientColor = 0 << 24 | acrylicBackgroundColor & 0xFFFFFF
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
        #endregion
    }
}
