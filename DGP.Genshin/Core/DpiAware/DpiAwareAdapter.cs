using Snap.Core.Logging;
using Snap.Win32;
using Snap.Win32.NativeMethod;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace DGP.Genshin.Core.DpiAware
{
    /// <summary>
    /// 高分辨率适配器
    /// </summary>
    internal class DpiAwareAdapter
    {
        private readonly Window window;

        private HwndSource? hwndSource;
        [SuppressMessage("", "IDE0052")]
        private IntPtr? hwnd;
        private double currentDpiRatio;

        static DpiAwareAdapter()
        {
            if (DpiAware.IsSupported)
            {
                // We need to call this early before we start doing any fiddling with window coordinates / geometry
                _ = SHCore.SetProcessDpiAwareness(SHCore.PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
            }
        }

        /// <summary>
        /// 构造一个新的高分辨率适配器
        /// </summary>
        /// <param name="window">目标窗体</param>
        public DpiAwareAdapter(Window window)
        {
            this.window = window;
            window.Loaded += (_, _) => this.OnAttached();
            window.Closing += (_, _) => this.OnDetaching();
        }

        private void OnAttached()
        {
            if (this.window.IsInitialized)
            {
                this.AddHwndHook();
            }
            else
            {
                this.window.SourceInitialized += this.AssociatedWindowSourceInitialized;
            }
        }

        private void OnDetaching()
        {
            this.RemoveHwndHook();
        }

        private void AddHwndHook()
        {
            this.hwndSource = PresentationSource.FromVisual(this.window) as HwndSource;
            this.hwndSource?.AddHook(this.HwndHook);
            this.hwnd = new WindowInteropHelper(this.window).Handle;
        }

        private void RemoveHwndHook()
        {
            this.window.SourceInitialized -= this.AssociatedWindowSourceInitialized;
            this.hwndSource?.RemoveHook(this.HwndHook);
            this.hwnd = null;
        }

        private void AssociatedWindowSourceInitialized(object? sender, EventArgs e)
        {
            this.AddHwndHook();

            this.currentDpiRatio = DpiAware.GetScaleRatio(this.window);
            this.UpdateDpiScaling(this.currentDpiRatio, true);
        }

        private IntPtr HwndHook(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (message is 0x02E0)
            {
                RECT rect = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT))!;

                User32.SetWindowPosFlags flag =
                    User32.SetWindowPosFlags.DoNotChangeOwnerZOrder
                    | User32.SetWindowPosFlags.DoNotActivate
                    | User32.SetWindowPosFlags.IgnoreZOrder;
                User32.SetWindowPos(hWnd, IntPtr.Zero, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, flag);

                // we modified this fragment to correct the wrong behaviour
                double newDpiRatio = DpiAware.GetScaleRatio(this.window) * this.currentDpiRatio;
                if (newDpiRatio != this.currentDpiRatio)
                {
                    this.UpdateDpiScaling(newDpiRatio);
                }
            }

            return IntPtr.Zero;
        }

        private void UpdateDpiScaling(double newDpiRatio, bool useSacleCenter = false)
        {
            this.currentDpiRatio = newDpiRatio;
            Logger.LogStatic($"Set dpi scaling to {this.currentDpiRatio:p2}");
            Visual firstChild = (Visual)VisualTreeHelper.GetChild(this.window, 0);
            ScaleTransform transform;
            if (useSacleCenter)
            {
                double centerX = this.window.Left + (this.window.Width / 2);
                double centerY = this.window.Top + (this.window.Height / 2);

                transform = new ScaleTransform(this.currentDpiRatio, this.currentDpiRatio, centerX, centerY);
            }
            else
            {
                transform = new ScaleTransform(this.currentDpiRatio, this.currentDpiRatio);
            }

            firstChild.SetValue(Window.LayoutTransformProperty, transform);
        }
    }
}