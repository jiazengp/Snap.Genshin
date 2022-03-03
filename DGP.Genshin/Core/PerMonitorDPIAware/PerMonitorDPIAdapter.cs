using Snap.Core.Logging;
using Snap.Win32.NativeMethod;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace DGP.Genshin.Core.PerMonitorDPIAware
{

    public class PerMonitorDPIAdapter
    {
        private HwndSource? hwndSource;
        [SuppressMessage("CodeQuality", "IDE0052:删除未读的私有成员", Justification = "<挂起>")]
        private IntPtr? hwnd;
        private double currentDpiRatio;
        private readonly Window AssociatedWindow;

        static PerMonitorDPIAdapter()
        {
            if (MonitorDPI.IsHighDpiMethodSupported())
            {
                // We need to call this early before we start doing any fiddling with window coordinates / geometry
                _ = SHCore.SetProcessDpiAwareness(SHCore.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
            }
        }

        public PerMonitorDPIAdapter(Window mainWindow)
        {
            AssociatedWindow = mainWindow;
            mainWindow.Loaded += (o, e) => OnAttached();
            mainWindow.Closing += (o, e) => OnDetaching();
        }

        protected void OnAttached()
        {
            if (AssociatedWindow.IsInitialized)
            {
                AddHwndHook();
            }
            else
            {
                AssociatedWindow.SourceInitialized += AssociatedWindowSourceInitialized;
            }
        }

        protected void OnDetaching()
        {
            RemoveHwndHook();
        }

        private void AddHwndHook()
        {
            hwndSource = PresentationSource.FromVisual(AssociatedWindow) as HwndSource;
            hwndSource?.AddHook(HwndHook);
            hwnd = new WindowInteropHelper(AssociatedWindow).Handle;
        }

        private void RemoveHwndHook()
        {
            AssociatedWindow.SourceInitialized -= AssociatedWindowSourceInitialized;
            hwndSource?.RemoveHook(HwndHook);
            hwnd = null;
        }

        private void AssociatedWindowSourceInitialized(object? sender, EventArgs e)
        {
            AddHwndHook();

            currentDpiRatio = MonitorDPI.GetScaleRatioForWindow(AssociatedWindow);
            UpdateDpiScaling(currentDpiRatio, true);
        }

        private IntPtr HwndHook(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (message)
            {
                case 0x02E0://WM_DPICHANGED
                    RECT rect = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT))!;

                    User32.SetWindowPos(hWnd, IntPtr.Zero, rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top,
                        User32.SetWindowPosFlags.DoNotChangeOwnerZOrder
                        | User32.SetWindowPosFlags.DoNotActivate
                        | User32.SetWindowPosFlags.IgnoreZOrder);

                    //we modified this fragment to correct the wrong behaviour
                    double newDpiRatio = MonitorDPI.GetScaleRatioForWindow(AssociatedWindow) * currentDpiRatio;
                    if (newDpiRatio != currentDpiRatio)
                    {
                        UpdateDpiScaling(newDpiRatio);
                    }

                    break;
            }

            return IntPtr.Zero;
        }

        private void UpdateDpiScaling(double newDpiRatio, bool useSacleCenter = false)
        {
            currentDpiRatio = newDpiRatio;
            Logger.LogStatic($"Set dpi scaling to {currentDpiRatio:p2}");
            Visual firstChild = (Visual)VisualTreeHelper.GetChild(AssociatedWindow, 0);
            ScaleTransform transform;
            if (useSacleCenter)
            {
                double centerX = AssociatedWindow.Left + AssociatedWindow.Width / 2;
                double centerY = AssociatedWindow.Top + AssociatedWindow.Height / 2;
                transform = new ScaleTransform(currentDpiRatio, currentDpiRatio, centerX, centerY);
            }
            else
            {
                transform = new ScaleTransform(currentDpiRatio, currentDpiRatio);
            }
            firstChild.SetValue(Window.LayoutTransformProperty, transform);
        }
    }
}
