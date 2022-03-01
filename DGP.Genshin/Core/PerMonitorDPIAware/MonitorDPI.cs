using Snap.Win32.NativeMethod;
using System;
using System.Diagnostics;
using System.Security;
using System.Windows;
using System.Windows.Interop;

namespace DGP.Genshin.Core.PerMonitorDPIAware
{
    public static class MonitorDPI
    {
        private static bool? isHighDpiMethodSupported = null;
        public static bool IsHighDpiMethodSupported()
        {
            isHighDpiMethodSupported ??= DoesWin32MethodExist("shcore.dll", "SetProcessDpiAwareness");
            return isHighDpiMethodSupported.Value;
        }

        public static double GetScaleRatioForWindow(IntPtr hWnd)
        {
            double wpfDpi = 96.0 * PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice.M11;

            if (IsHighDpiMethodSupported() == false)
            {
                // Use System DPI
                return wpfDpi / 96.0;
            }
            else
            {
                IntPtr monitor = User32.MonitorFromWindow(hWnd, User32.MonitorOpts.MONITOR_DEFAULTTONEAREST);
                _ = SHCore.GetDpiForMonitor(monitor, SHCore.MonitorDpiType.MDT_EFFECTIVE_DPI, out uint dpiX, out uint _);
                double targetDpi = dpiX / wpfDpi;
                return dpiX / wpfDpi;
            }
        }

        public static double GetScaleRatioForWindow(Window window)
        {
            HwndSource? hwndSource = PresentationSource.FromVisual(window) as HwndSource;
            return GetScaleRatioForWindow(hwndSource!.Handle);
        }

        [SecurityCritical]  // auto-generated
        internal static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr hModule = Kernel32.LoadLibrary(moduleName);

            if (hModule == IntPtr.Zero)
            {
                Debug.Assert(hModule != IntPtr.Zero, "LoadLibrary failed. API must not be available");
                return false;
            }

            IntPtr functionPointer = Kernel32.GetProcAddress(hModule, methodName);

            Kernel32.FreeLibrary(hModule);
            return (functionPointer != IntPtr.Zero);
        }
    }
}
