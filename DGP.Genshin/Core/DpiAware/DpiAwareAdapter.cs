using Snap.Core.Logging;
using Snap.Win32.NativeMethod;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace DGP.Genshin.Core.DpiAware
{

    internal class DpiAwareAdapter
    {
        private HwndSource? hwndSource;
        [SuppressMessage("", "IDE0052")]
        private IntPtr? hwnd;
        private double currentDpiRatio;
        private readonly Window Window;

        static DpiAwareAdapter()
        {
            if (DpiAware.IsSupported)
            {
                // We need to call this early before we start doing any fiddling with window coordinates / geometry
                _ = SHCore.SetProcessDpiAwareness(SHCore.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
            }
        }

        public DpiAwareAdapter(Window mainWindow)
        {
            Window = mainWindow;
            mainWindow.Loaded += (_, _) => OnAttached();
            mainWindow.Closing += (_, _) => OnDetaching();
        }

        protected void OnAttached()
        {
            if (Window.IsInitialized)
            {
                AddHwndHook();
            }
            else
            {
                Window.SourceInitialized += AssociatedWindowSourceInitialized;
            }
        }

        protected void OnDetaching()
        {
            RemoveHwndHook();
        }

        private void AddHwndHook()
        {
            hwndSource = PresentationSource.FromVisual(Window) as HwndSource;
            hwndSource?.AddHook(HwndHook);
            hwnd = new WindowInteropHelper(Window).Handle;
        }

        private void RemoveHwndHook()
        {
            Window.SourceInitialized -= AssociatedWindowSourceInitialized;
            hwndSource?.RemoveHook(HwndHook);
            hwnd = null;
        }

        private void AssociatedWindowSourceInitialized(object? sender, EventArgs e)
        {
            AddHwndHook();

            currentDpiRatio = DpiAware.GetScaleRatio(Window);
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
                    double newDpiRatio = DpiAware.GetScaleRatio(Window) * currentDpiRatio;
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
            Visual firstChild = (Visual)VisualTreeHelper.GetChild(Window, 0);
            ScaleTransform transform;
            if (useSacleCenter)
            {
                double centerX = Window.Left + Window.Width / 2;
                double centerY = Window.Top + Window.Height / 2;
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
