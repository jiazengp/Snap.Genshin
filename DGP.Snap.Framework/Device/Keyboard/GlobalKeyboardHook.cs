using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static DGP.Snap.Framework.NativeMethods.Kernel32;
using static DGP.Snap.Framework.NativeMethods.User32;

namespace DGP.Snap.Framework.Device.Keyboard
{

    //Based on https://gist.github.com/Stasonix
    public class GlobalKeyboardHook : IDisposable
    {
        public event EventHandler<GlobalKeyboardHookEventArgs> KeyboardPressed;

        public GlobalKeyboardHook()
        {
            this._windowsHookHandle = IntPtr.Zero;
            this._user32LibraryHandle = IntPtr.Zero;
            this._hookProc = LowLevelKeyboardProc; // we must keep alive _hookProc, because GC is not aware about SetWindowsHookEx behaviour.

            this._user32LibraryHandle = LoadLibrary("User32");
            if (this._user32LibraryHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to load library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }

            this._windowsHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, this._hookProc, this._user32LibraryHandle, 0);
            if (this._windowsHookHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to adjust keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // because we can unhook only in the same thread, not in garbage collector thread
                if (this._windowsHookHandle != IntPtr.Zero)
                {
                    if (!UnhookWindowsHookEx(this._windowsHookHandle))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errorCode, $"Failed to remove keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                    }
                    this._windowsHookHandle = IntPtr.Zero;

                    // ReSharper disable once DelegateSubtraction
                    this._hookProc -= LowLevelKeyboardProc;
                }
            }

            if (this._user32LibraryHandle != IntPtr.Zero)
            {
                if (!FreeLibrary(this._user32LibraryHandle)) // reduces reference to library by 1.
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, $"Failed to unload library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                }
                this._user32LibraryHandle = IntPtr.Zero;
            }
        }

        ~GlobalKeyboardHook()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private IntPtr _windowsHookHandle;
        private IntPtr _user32LibraryHandle;
        private HookProc _hookProc;

        public const int WH_KEYBOARD_LL = 13;
        //const int HC_ACTION = 0;
        public const int VkSnapshot = 0x2c;

        public const int VkLwin = 0x5b;
        public const int VkRwin = 0x5c;
        public const int VkTab = 0x09;
        public const int VkEscape = 0x18;
        public const int VkControl = 0x11;
        private const int KfAltdown = 0x2000;
        public const int LlkhfAltdown = KfAltdown >> 8;

        public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool fEatKeyStroke = false;

            int wparamTyped = wParam.ToInt32();
            if (Enum.IsDefined(typeof(KeyboardState), wparamTyped))
            {
                object o = Marshal.PtrToStructure(lParam, typeof(LowLevelKeyboardInputEvent));
                LowLevelKeyboardInputEvent p = (LowLevelKeyboardInputEvent)o;

                GlobalKeyboardHookEventArgs eventArguments = new GlobalKeyboardHookEventArgs(p, (KeyboardState)wparamTyped);

                EventHandler<GlobalKeyboardHookEventArgs> handler = KeyboardPressed;
                handler?.Invoke(this, eventArguments);

                fEatKeyStroke = eventArguments.Handled;
            }

            return fEatKeyStroke ? (IntPtr)1 : CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
    }
}
